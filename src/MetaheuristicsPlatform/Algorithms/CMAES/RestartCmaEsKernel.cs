using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

internal enum RestartCmaEsStrategy
{
    Ipop = 0,
    Bipop = 1
}

internal static class RestartCmaEsKernel
{
    private readonly record struct RunPlan(
        int RestartIndex,
        RestartCmaEsRegime Regime,
        int PopulationSize,
        double InitialStepSize,
        double[] InitialMean);

    private readonly record struct RunOutcome(
        bool GlobalStop,
        StoppingDecision StopDecision,
        long EvaluationCount,
        int CompleteGenerations,
        double LastSigma,
        double LastConditionNumber);

    public static OptimizationResult<double[]> Optimize(
        MetaheuristicDescriptor descriptor,
        RestartCmaEsStrategy strategy,
        IOptimizationProblem<double[]> problem,
        RestartCmaEsParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options,
        IOptimizationCallback<double[]>? callback,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);

        parameters.Validate();

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "Restart CMA-ES requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Restart CMA-ES requires a positive search-space dimension.");
        }

        int lambda0 =
            parameters.InitialPopulationSize > 0
                ? parameters.InitialPopulationSize
                : Math.Max(
                    2,
                    4 +
                    (int)Math.Floor(
                        3.0 *
                        Math.Log(dimension)));

        double sigma0 =
            ResolveInitialStepSize(
                searchSpace,
                parameters.InitialStepSize);

        double[] firstMean =
            ResolveFirstMean(
                searchSpace,
                parameters.InitialMean);

        var context =
            new OptimizationContext<double[]>(
                descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        long largeBudget = 0;
        long smallBudget = 0;
        int largeRestartIndex = 0;
        int totalRuns =
            parameters.MaximumRestarts + 1;

        RunPlan lastPlan =
            new(
                RestartIndex: 0,
                Regime: RestartCmaEsRegime.Initial,
                PopulationSize: lambda0,
                InitialStepSize: sigma0,
                InitialMean: firstMean);

        RunOutcome lastOutcome =
            new(
                GlobalStop: false,
                StopDecision:
                    StoppingDecision.Continue(
                        "NotStarted"),
                EvaluationCount: 0,
                CompleteGenerations: 0,
                LastSigma: sigma0,
                LastConditionNumber: 1.0);

        RestartCmaEsState initialState =
            new(
                RestartIndex: 0,
                Regime: RestartCmaEsRegime.Initial,
                GenerationInRestart: 0,
                PopulationSize: lambda0,
                ParentCount: Math.Max(1, lambda0 / 2),
                StepSize: sigma0,
                ConditionNumberEstimate: 1.0,
                LargePopulationEvaluationBudget: 0,
                SmallPopulationEvaluationBudget: 0,
                GenerationBestFitness: null);

        context.Start(initialState);

        for (int run = 0; run < totalRuns; run++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RunPlan plan;

            if (run == 0)
            {
                plan =
                    new RunPlan(
                        RestartIndex: 0,
                        Regime: RestartCmaEsRegime.Initial,
                        PopulationSize: lambda0,
                        InitialStepSize: sigma0,
                        InitialMean: firstMean);
            }
            else if (strategy == RestartCmaEsStrategy.Ipop)
            {
                largeRestartIndex++;

                plan =
                    new RunPlan(
                        RestartIndex: run,
                        Regime: RestartCmaEsRegime.LargePopulation,
                        PopulationSize:
                            ResolveLargePopulation(
                                lambda0,
                                largeRestartIndex,
                                parameters.PopulationMultiplier),
                        InitialStepSize: sigma0,
                        InitialMean:
                            SampleUniformMean(
                                searchSpace,
                                context.Random));
            }
            else
            {
                bool chooseLarge =
                    largeBudget <= smallBudget;

                if (chooseLarge)
                {
                    largeRestartIndex++;

                    plan =
                        new RunPlan(
                            RestartIndex: run,
                            Regime: RestartCmaEsRegime.LargePopulation,
                            PopulationSize:
                                ResolveLargePopulation(
                                    lambda0,
                                    largeRestartIndex,
                                    parameters.PopulationMultiplier),
                            InitialStepSize: sigma0,
                            InitialMean:
                                SampleUniformMean(
                                    searchSpace,
                                    context.Random));
                }
                else
                {
                    int currentLargePopulation =
                        ResolveLargePopulation(
                            lambda0,
                            Math.Max(1, largeRestartIndex),
                            parameters.PopulationMultiplier);

                    plan =
                        new RunPlan(
                            RestartIndex: run,
                            Regime: RestartCmaEsRegime.SmallPopulation,
                            PopulationSize:
                                ResolveBipopSmallPopulation(
                                    lambda0,
                                    currentLargePopulation,
                                    context.Random.NextDouble()),
                            InitialStepSize:
                                sigma0 *
                                Math.Pow(
                                    10.0,
                                    -2.0 *
                                    context.Random.NextDouble()),
                            InitialMean:
                                SampleUniformMean(
                                    searchSpace,
                                    context.Random));
                }
            }

            long evaluationsBefore =
                context.State.Evaluations;

            RunOutcome outcome =
                RunOne(
                    context,
                    continuousProblem,
                    parameters,
                    plan,
                    largeBudget,
                    smallBudget,
                    cancellationToken);

            lastPlan = plan;
            lastOutcome = outcome;

            long evaluationsUsed =
                context.State.Evaluations -
                evaluationsBefore;

            if (strategy == RestartCmaEsStrategy.Ipop)
            {
                largeBudget += evaluationsUsed;
            }
            else if (plan.Regime == RestartCmaEsRegime.LargePopulation)
            {
                largeBudget += evaluationsUsed;
            }
            else
            {
                // In BIPOP, the initial run is accounted on the small-budget
                // side. This guarantees that the first actual restart enters
                // the large/IPOP regime and follows common BIPOP practice.
                smallBudget += evaluationsUsed;
            }

            if (outcome.GlobalStop)
            {
                RestartCmaEsState finalState =
                    new(
                        RestartIndex: plan.RestartIndex,
                        Regime: plan.Regime,
                        GenerationInRestart: outcome.CompleteGenerations,
                        PopulationSize: plan.PopulationSize,
                        ParentCount:
                            Math.Max(
                                1,
                                plan.PopulationSize / 2),
                        StepSize: outcome.LastSigma,
                        ConditionNumberEstimate:
                            outcome.LastConditionNumber,
                        LargePopulationEvaluationBudget: largeBudget,
                        SmallPopulationEvaluationBudget: smallBudget,
                        GenerationBestFitness: null);

                return context.Complete(
                    outcome.StopDecision,
                    finalState);
            }
        }

        string criterion =
            strategy == RestartCmaEsStrategy.Ipop
                ? "MaximumIpopCmaEsRestarts"
                : "MaximumBipopCmaEsRestarts";

        string message =
            strategy == RestartCmaEsStrategy.Ipop
                ? "The configured IPOP-CMA-ES restart limit was reached."
                : "The configured BIPOP-CMA-ES restart limit was reached.";

        RestartCmaEsState completedState =
            new(
                RestartIndex: lastPlan.RestartIndex,
                Regime: lastPlan.Regime,
                GenerationInRestart: lastOutcome.CompleteGenerations,
                PopulationSize: lastPlan.PopulationSize,
                ParentCount:
                    Math.Max(
                        1,
                        lastPlan.PopulationSize / 2),
                StepSize: lastOutcome.LastSigma,
                ConditionNumberEstimate:
                    lastOutcome.LastConditionNumber,
                LargePopulationEvaluationBudget: largeBudget,
                SmallPopulationEvaluationBudget: smallBudget,
                GenerationBestFitness: null);

        return context.Complete(
            StoppingDecision.Stop(
                criterion,
                message),
            completedState);
    }

    private static RunOutcome RunOne(
        OptimizationContext<double[]> context,
        ISpanContinuousOptimizationProblem continuousProblem,
        RestartCmaEsParameters parameters,
        RunPlan plan,
        long largeBudget,
        long smallBudget,
        CancellationToken cancellationToken)
    {
        int dimension =
            continuousProblem.SearchSpace.Dimension;

        int lambda =
            plan.PopulationSize;

        int mu =
            Math.Max(
                1,
                lambda / 2);

        double[] weights =
            BuildPositiveWeights(mu);

        double squaredWeightSum = 0.0;

        for (int i = 0; i < weights.Length; i++)
        {
            squaredWeightSum +=
                weights[i] *
                weights[i];
        }

        double muEffective =
            1.0 /
            squaredWeightSum;

        double n =
            dimension;

        double cSigma =
            (muEffective + 2.0) /
            (n + muEffective + 5.0);

        double dSigma =
            1.0 +
            (2.0 *
             Math.Max(
                 0.0,
                 Math.Sqrt(
                     (muEffective - 1.0) /
                     (n + 1.0)) -
                 1.0)) +
            cSigma;

        double cC =
            (4.0 + (muEffective / n)) /
            (n + 4.0 +
             ((2.0 * muEffective) / n));

        double c1 =
            2.0 /
            (Math.Pow(n + 1.3, 2.0) +
             muEffective);

        double cMu =
            Math.Min(
                1.0 - c1,
                (2.0 *
                 (muEffective -
                  2.0 +
                  (1.0 / muEffective))) /
                (Math.Pow(n + 2.0, 2.0) +
                 muEffective));

        double chiN =
            Math.Sqrt(n) *
            (1.0 -
             (1.0 / (4.0 * n)) +
             (1.0 / (21.0 * n * n)));

        double[] mean =
            (double[])plan.InitialMean.Clone();

        double sigma =
            plan.InitialStepSize;

        double[] covariance =
            new double[dimension * dimension];

        double[] eigenvectors =
            new double[dimension * dimension];

        double[] axisScales =
            new double[dimension];

        double[] pSigma =
            new double[dimension];

        double[] pC =
            new double[dimension];

        for (int i = 0; i < dimension; i++)
        {
            covariance[(i * dimension) + i] = 1.0;
            eigenvectors[(i * dimension) + i] = 1.0;
            axisScales[i] = 1.0;
        }

        double conditionNumber = 1.0;

        double[][] population =
            new double[lambda][];

        double[][] normalizedSteps =
            new double[lambda][];

        double[] fitness =
            new double[lambda];

        int[] order =
            new int[lambda];

        for (int k = 0; k < lambda; k++)
        {
            population[k] =
                new double[dimension];

            normalizedSteps[k] =
                new double[dimension];

            order[k] = k;
        }

        double[] z =
            new double[dimension];

        double[] transformed =
            new double[dimension];

        double[] deltaMean =
            new double[dimension];

        double[] whitenedDelta =
            new double[dimension];

        var gaussian =
            new CmaEsGaussianSampler();

        long evaluationsBefore =
            context.State.Evaluations;

        for (int generation = 1;
             generation <= parameters.MaximumGenerationsPerRestart;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double[] oldMean =
                (double[])mean.Clone();

            double? generationBest =
                null;

            for (int k = 0; k < lambda; k++)
            {
                gaussian.Fill(
                    context.Random,
                    z);

                CmaEsSymmetricEigenSolver.Transform(
                    eigenvectors,
                    axisScales,
                    z,
                    transformed);

                double[] candidate =
                    population[k];

                for (int i = 0; i < dimension; i++)
                {
                    candidate[i] =
                        oldMean[i] +
                        (sigma * transformed[i]);
                }

                continuousProblem.SearchSpace.Clamp(
                    candidate.AsSpan());

                double[] step =
                    normalizedSteps[k];

                for (int i = 0; i < dimension; i++)
                {
                    step[i] =
                        (candidate[i] -
                         oldMean[i]) /
                        sigma;
                }

                RestartCmaEsState state =
                    new(
                        RestartIndex: plan.RestartIndex,
                        Regime: plan.Regime,
                        GenerationInRestart: generation - 1,
                        PopulationSize: lambda,
                        ParentCount: mu,
                        StepSize: sigma,
                        ConditionNumberEstimate: conditionNumber,
                        LargePopulationEvaluationBudget: largeBudget,
                        SmallPopulationEvaluationBudget: smallBudget,
                        GenerationBestFitness: generationBest);

                double objective =
                    context.Evaluate(
                        candidate,
                        state);

                if (!double.IsFinite(objective))
                {
                    throw new InvalidOperationException(
                        "Restart CMA-ES requires finite objective values.");
                }

                fitness[k] =
                    objective;

                if (!generationBest.HasValue ||
                    continuousProblem.Sense.IsBetter(
                        objective,
                        generationBest.Value))
                {
                    generationBest =
                        objective;
                }

                StoppingDecision partialStop =
                    context.EvaluateStopping(state);

                if (partialStop.ShouldStop)
                {
                    return
                        new RunOutcome(
                            GlobalStop: true,
                            StopDecision: partialStop,
                            EvaluationCount:
                                context.State.Evaluations -
                                evaluationsBefore,
                            CompleteGenerations:
                                generation - 1,
                            LastSigma: sigma,
                            LastConditionNumber: conditionNumber);
                }
            }

            Array.Sort(
                order,
                (left, right) =>
                    CompareFitness(
                        continuousProblem.Sense,
                        fitness[left],
                        fitness[right]));

            Array.Clear(
                mean,
                0,
                mean.Length);

            for (int rank = 0; rank < mu; rank++)
            {
                int index =
                    order[rank];

                double weight =
                    weights[rank];

                double[] candidate =
                    population[index];

                for (int i = 0; i < dimension; i++)
                {
                    mean[i] +=
                        weight *
                        candidate[i];
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                deltaMean[i] =
                    (mean[i] -
                     oldMean[i]) /
                    sigma;
            }

            CmaEsSymmetricEigenSolver.ApplyInverseSquareRoot(
                eigenvectors,
                axisScales,
                deltaMean,
                whitenedDelta);

            double sigmaPathScale =
                Math.Sqrt(
                    cSigma *
                    (2.0 - cSigma) *
                    muEffective);

            for (int i = 0; i < dimension; i++)
            {
                pSigma[i] =
                    ((1.0 - cSigma) *
                     pSigma[i]) +
                    (sigmaPathScale *
                     whitenedDelta[i]);
            }

            double pSigmaNorm =
                EuclideanNorm(pSigma);

            double decayCorrection =
                Math.Sqrt(
                    1.0 -
                    Math.Pow(
                        1.0 - cSigma,
                        2.0 * generation));

            double normalizedPath =
                pSigmaNorm /
                (decayCorrection * chiN);

            bool hSigma =
                normalizedPath <
                (1.4 +
                 (2.0 / (n + 1.0)));

            double covariancePathScale =
                hSigma
                    ? Math.Sqrt(
                          cC *
                          (2.0 - cC) *
                          muEffective)
                    : 0.0;

            for (int i = 0; i < dimension; i++)
            {
                pC[i] =
                    ((1.0 - cC) *
                     pC[i]) +
                    (covariancePathScale *
                     deltaMean[i]);
            }

            double oldCovarianceFactor =
                1.0 -
                c1 -
                cMu +
                (hSigma
                    ? 0.0
                    : c1 *
                      cC *
                      (2.0 - cC));

            for (int row = 0;
                 row < dimension;
                 row++)
            {
                for (int column = 0;
                     column <= row;
                     column++)
                {
                    int rc =
                        (row * dimension) +
                        column;

                    double updated =
                        oldCovarianceFactor *
                        covariance[rc];

                    updated +=
                        c1 *
                        pC[row] *
                        pC[column];

                    double rankMu = 0.0;

                    for (int rank = 0;
                         rank < mu;
                         rank++)
                    {
                        int index =
                            order[rank];

                        double[] step =
                            normalizedSteps[index];

                        rankMu +=
                            weights[rank] *
                            step[row] *
                            step[column];
                    }

                    updated +=
                        cMu *
                        rankMu;

                    if (!double.IsFinite(updated))
                    {
                        throw new InvalidOperationException(
                            "Restart CMA-ES covariance update became non-finite.");
                    }

                    covariance[rc] =
                        updated;

                    covariance[
                        (column * dimension) +
                        row] =
                        updated;
                }
            }

            sigma *=
                Math.Exp(
                    (cSigma / dSigma) *
                    ((pSigmaNorm / chiN) -
                     1.0));

            if (!double.IsFinite(sigma) ||
                sigma <= 0.0)
            {
                throw new InvalidOperationException(
                    "Restart CMA-ES step-size adaptation produced an invalid sigma.");
            }

            conditionNumber =
                CmaEsSymmetricEigenSolver.Decompose(
                    covariance,
                    dimension,
                    parameters.MinimumCovarianceEigenvalue,
                    eigenvectors,
                    axisScales);

            CmaEsSymmetricEigenSolver.ReconstructPositiveDefinite(
                eigenvectors,
                axisScales,
                covariance);

            if (!double.IsFinite(conditionNumber) ||
                conditionNumber <= 0.0)
            {
                throw new InvalidOperationException(
                    "Restart CMA-ES covariance condition estimate is invalid.");
            }

            RestartCmaEsState completed =
                new(
                    RestartIndex: plan.RestartIndex,
                    Regime: plan.Regime,
                    GenerationInRestart: generation,
                    PopulationSize: lambda,
                    ParentCount: mu,
                    StepSize: sigma,
                    ConditionNumberEstimate: conditionNumber,
                    LargePopulationEvaluationBudget: largeBudget,
                    SmallPopulationEvaluationBudget: smallBudget,
                    GenerationBestFitness: generationBest);

            context.CompleteIteration(
                generationBest,
                completed);

            StoppingDecision globalStop =
                context.EvaluateStopping(completed);

            if (globalStop.ShouldStop)
            {
                return
                    new RunOutcome(
                        GlobalStop: true,
                        StopDecision: globalStop,
                        EvaluationCount:
                            context.State.Evaluations -
                            evaluationsBefore,
                        CompleteGenerations: generation,
                        LastSigma: sigma,
                        LastConditionNumber: conditionNumber);
            }

            if (conditionNumber >=
                parameters.RestartConditionNumberThreshold)
            {
                return
                    new RunOutcome(
                        GlobalStop: false,
                        StopDecision:
                            StoppingDecision.Continue(
                                "RestartConditionNumber"),
                        EvaluationCount:
                            context.State.Evaluations -
                            evaluationsBefore,
                        CompleteGenerations: generation,
                        LastSigma: sigma,
                        LastConditionNumber: conditionNumber);
            }
        }

        return
            new RunOutcome(
                GlobalStop: false,
                StopDecision:
                    StoppingDecision.Continue(
                        "RestartGenerationLimit"),
                EvaluationCount:
                    context.State.Evaluations -
                    evaluationsBefore,
                CompleteGenerations:
                    parameters.MaximumGenerationsPerRestart,
                LastSigma: sigma,
                LastConditionNumber: conditionNumber);
    }

    private static int ResolveLargePopulation(
        int initialPopulation,
        int largeRestartIndex,
        double multiplier)
    {
        double resolved =
            initialPopulation *
            Math.Pow(
                multiplier,
                largeRestartIndex);

        if (!double.IsFinite(resolved) ||
            resolved > int.MaxValue)
        {
            throw new InvalidOperationException(
                "Restart CMA-ES population schedule exceeds Int32 capacity.");
        }

        return
            Math.Max(
                2,
                checked((int)Math.Round(
                    resolved,
                    MidpointRounding.AwayFromZero)));
    }

    private static int ResolveBipopSmallPopulation(
        int lambda0,
        int lambdaLarge,
        double uniform)
    {
        double exponent =
            uniform *
            uniform;

        double ratio =
            0.5 *
            lambdaLarge /
            lambda0;

        double resolved =
            lambda0 *
            Math.Pow(
                Math.Max(1.0, ratio),
                exponent);

        int population =
            checked((int)Math.Floor(resolved));

        int upper =
            Math.Max(
                lambda0,
                lambdaLarge / 2);

        return
            Math.Clamp(
                population,
                lambda0,
                upper);
    }

    private static double[] ResolveFirstMean(
        IBoundedContinuousSearchSpace searchSpace,
        double[]? configured)
    {
        int dimension =
            searchSpace.Dimension;

        if (configured is not null)
        {
            if (configured.Length != dimension)
            {
                throw new ArgumentException(
                    "InitialMean dimension does not match the search space.",
                    nameof(configured));
            }

            double[] mean =
                (double[])configured.Clone();

            if (!searchSpace.Contains(mean))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(configured),
                    "InitialMean must belong to the bounded search space.");
            }

            return mean;
        }

        double[] center =
            new double[dimension];

        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        for (int i = 0; i < dimension; i++)
        {
            center[i] =
                0.5 *
                (lower[i] + upper[i]);
        }

        return center;
    }

    private static double[] SampleUniformMean(
        IBoundedContinuousSearchSpace searchSpace,
        MetaheuristicsPlatform.Random.IRandomSource random)
    {
        double[] mean =
            new double[searchSpace.Dimension];

        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        for (int i = 0; i < mean.Length; i++)
        {
            mean[i] =
                lower[i] +
                (random.NextDouble() *
                 (upper[i] - lower[i]));
        }

        return mean;
    }

    private static double ResolveInitialStepSize(
        IBoundedContinuousSearchSpace searchSpace,
        double? configured)
    {
        if (configured.HasValue)
        {
            return configured.Value;
        }

        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        double squaredWidthSum = 0.0;

        for (int i = 0; i < searchSpace.Dimension; i++)
        {
            double width =
                upper[i] -
                lower[i];

            squaredWidthSum +=
                width *
                width;
        }

        double rmsWidth =
            Math.Sqrt(
                squaredWidthSum /
                searchSpace.Dimension);

        double sigma =
            0.3 *
            rmsWidth;

        if (!double.IsFinite(sigma) ||
            sigma <= 0.0)
        {
            throw new InvalidOperationException(
                "Unable to derive a valid Restart CMA-ES initial step size.");
        }

        return sigma;
    }

    private static double[] BuildPositiveWeights(
        int parentCount)
    {
        double[] weights =
            new double[parentCount];

        double sum = 0.0;

        for (int rank = 0;
             rank < parentCount;
             rank++)
        {
            double weight =
                Math.Log(
                    parentCount + 0.5) -
                Math.Log(rank + 1.0);

            weights[rank] =
                weight;

            sum +=
                weight;
        }

        for (int rank = 0;
             rank < parentCount;
             rank++)
        {
            weights[rank] /=
                sum;
        }

        return weights;
    }

    private static double EuclideanNorm(
        ReadOnlySpan<double> vector)
    {
        double squared = 0.0;

        for (int i = 0; i < vector.Length; i++)
        {
            squared +=
                vector[i] *
                vector[i];
        }

        return Math.Sqrt(squared);
    }

    private static int CompareFitness(
        OptimizationSense sense,
        double left,
        double right)
    {
        if (sense.IsBetter(left, right))
        {
            return -1;
        }

        if (sense.IsBetter(right, left))
        {
            return 1;
        }

        return 0;
    }
}
