using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

internal enum AdvancedCmaEsMode
{
    ActiveFullCovariance = 0,
    SeparableCovariance = 1
}

internal static class AdvancedCmaEsKernel
{
    public static OptimizationResult<double[]> Optimize(
        MetaheuristicDescriptor descriptor,
        AdvancedCmaEsMode mode,
        IOptimizationProblem<double[]> problem,
        CmaEsParameters parameters,
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
                "Advanced CMA-ES requires ISpanContinuousOptimizationProblem.");
        }

        int dimension = continuousProblem.SearchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Advanced CMA-ES requires a positive search-space dimension.");
        }

        int lambda =
            parameters.PopulationSize > 0
                ? parameters.PopulationSize
                : 4 + (int)Math.Floor(3.0 * Math.Log(dimension));

        lambda = Math.Max(2, lambda);

        int mu =
            parameters.ParentCount > 0
                ? parameters.ParentCount
                : lambda / 2;

        if (mu <= 0 || mu > lambda)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameters.ParentCount),
                "Resolved CMA-ES parent count must lie in [1,lambda].");
        }

        if (mode == AdvancedCmaEsMode.ActiveFullCovariance &&
            mu >= lambda)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameters.ParentCount),
                "Active CMA-ES requires at least one non-parent offspring.");
        }

        double[] positiveWeights = BuildPositiveWeights(mu);
        double squaredWeightSum = 0.0;

        for (int i = 0; i < positiveWeights.Length; i++)
        {
            squaredWeightSum +=
                positiveWeights[i] *
                positiveWeights[i];
        }

        double muEffective =
            1.0 / squaredWeightSum;

        double n = dimension;

        double cSigma =
            mode == AdvancedCmaEsMode.SeparableCovariance
                ? (muEffective + 2.0) /
                  (n + muEffective + 3.0)
                : (muEffective + 2.0) /
                  (n + muEffective + 5.0);

        double dSigma =
            1.0 +
            cSigma +
            (2.0 *
             Math.Max(
                 0.0,
                 Math.Sqrt(
                     (muEffective - 1.0) /
                     (n + 1.0)) -
                 1.0));

        double cC =
            mode == AdvancedCmaEsMode.SeparableCovariance
                ? 4.0 / (n + 4.0)
                : (4.0 + (muEffective / n)) /
                  (n + 4.0 + ((2.0 * muEffective) / n));

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

        double cCovSeparable =
            ResolveSeparableCovarianceLearningRate(
                n,
                muEffective);

        double[] activeWeights =
            mode == AdvancedCmaEsMode.ActiveFullCovariance
                ? BuildActiveWeights(
                    lambda,
                    mu,
                    positiveWeights,
                    n,
                    muEffective,
                    c1,
                    cMu)
                : positiveWeights;

        double chiN =
            Math.Sqrt(n) *
            (1.0 -
             (1.0 / (4.0 * n)) +
             (1.0 / (21.0 * n * n)));

        double[] mean =
            ResolveInitialMean(
                continuousProblem.SearchSpace,
                parameters);

        double sigma =
            ResolveInitialStepSize(
                continuousProblem.SearchSpace,
                parameters);

        double[] pSigma = new double[dimension];
        double[] pC = new double[dimension];

        double[] covariance =
            mode == AdvancedCmaEsMode.ActiveFullCovariance
                ? new double[dimension * dimension]
                : Array.Empty<double>();

        double[] eigenvectors =
            mode == AdvancedCmaEsMode.ActiveFullCovariance
                ? new double[dimension * dimension]
                : Array.Empty<double>();

        double[] axisScales =
            new double[dimension];

        double[] variances =
            mode == AdvancedCmaEsMode.SeparableCovariance
                ? new double[dimension]
                : Array.Empty<double>();

        for (int i = 0; i < dimension; i++)
        {
            axisScales[i] = 1.0;

            if (mode == AdvancedCmaEsMode.ActiveFullCovariance)
            {
                covariance[(i * dimension) + i] = 1.0;
                eigenvectors[(i * dimension) + i] = 1.0;
            }
            else
            {
                variances[i] = 1.0;
            }
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
            population[k] = new double[dimension];
            normalizedSteps[k] = new double[dimension];
            order[k] = k;
        }

        double[] z = new double[dimension];
        double[] transformed = new double[dimension];
        double[] deltaMean = new double[dimension];
        double[] whitenedDelta = new double[dimension];
        double[] negativeWhitened = new double[dimension];
        double[] activeStepScale = new double[lambda];

        var gaussian =
            new CmaEsGaussianSampler();

        var context =
            new OptimizationContext<double[]>(
                descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        CmaEsState state =
            new(
                Generation: 0,
                PopulationSize: lambda,
                ParentCount: mu,
                StepSize: sigma,
                ConditionNumberEstimate: conditionNumber,
                GenerationBestFitness: null);

        context.Start(state);

        for (int generation = 1;
             generation <= parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double[] oldMean =
                (double[])mean.Clone();

            double? generationBest = null;

            for (int k = 0; k < lambda; k++)
            {
                gaussian.Fill(
                    context.Random,
                    z);

                if (mode == AdvancedCmaEsMode.ActiveFullCovariance)
                {
                    CmaEsSymmetricEigenSolver.Transform(
                        eigenvectors,
                        axisScales,
                        z,
                        transformed);
                }
                else
                {
                    for (int i = 0; i < dimension; i++)
                    {
                        transformed[i] =
                            axisScales[i] *
                            z[i];
                    }
                }

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

                state =
                    new CmaEsState(
                        Generation: generation - 1,
                        PopulationSize: lambda,
                        ParentCount: mu,
                        StepSize: sigma,
                        ConditionNumberEstimate: conditionNumber,
                        GenerationBestFitness: generationBest);

                double objective =
                    context.Evaluate(
                        candidate,
                        state);

                if (!double.IsFinite(objective))
                {
                    throw new InvalidOperationException(
                        "Advanced CMA-ES requires finite objective values.");
                }

                fitness[k] = objective;

                if (!generationBest.HasValue ||
                    problem.Sense.IsBetter(
                        objective,
                        generationBest.Value))
                {
                    generationBest = objective;
                }

                StoppingDecision partialStop =
                    context.EvaluateStopping(state);

                if (partialStop.ShouldStop)
                {
                    // Never update the distribution from a partial generation.
                    return context.Complete(
                        partialStop,
                        state);
                }
            }

            Array.Sort(
                order,
                (left, right) =>
                    CompareFitness(
                        problem.Sense,
                        fitness[left],
                        fitness[right]));

            Array.Clear(
                mean,
                0,
                mean.Length);

            for (int rank = 0; rank < mu; rank++)
            {
                int index = order[rank];
                double weight = positiveWeights[rank];
                double[] candidate = population[index];

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

            if (mode == AdvancedCmaEsMode.ActiveFullCovariance)
            {
                CmaEsSymmetricEigenSolver.ApplyInverseSquareRoot(
                    eigenvectors,
                    axisScales,
                    deltaMean,
                    whitenedDelta);
            }
            else
            {
                for (int i = 0; i < dimension; i++)
                {
                    whitenedDelta[i] =
                        deltaMean[i] /
                        axisScales[i];
                }
            }

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

            if (mode == AdvancedCmaEsMode.ActiveFullCovariance)
            {
                UpdateActiveFullCovariance(
                    covariance,
                    eigenvectors,
                    axisScales,
                    normalizedSteps,
                    order,
                    activeWeights,
                    activeStepScale,
                    negativeWhitened,
                    dimension,
                    mu,
                    c1,
                    cMu,
                    cC,
                    hSigma,
                    pC);

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
            }
            else
            {
                conditionNumber =
                    UpdateSeparableCovariance(
                        variances,
                        axisScales,
                        normalizedSteps,
                        order,
                        positiveWeights,
                        dimension,
                        muEffective,
                        cCovSeparable,
                        cC,
                        hSigma,
                        pC,
                        parameters.MinimumCovarianceEigenvalue);
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
                    "Advanced CMA-ES step-size adaptation produced an invalid sigma.");
            }

            if (!double.IsFinite(conditionNumber) ||
                conditionNumber <= 0.0)
            {
                throw new InvalidOperationException(
                    "Advanced CMA-ES covariance condition estimate is invalid.");
            }

            state =
                new CmaEsState(
                    Generation: generation,
                    PopulationSize: lambda,
                    ParentCount: mu,
                    StepSize: sigma,
                    ConditionNumberEstimate: conditionNumber,
                    GenerationBestFitness: generationBest);

            context.CompleteIteration(
                generationBest,
                state);

            StoppingDecision generationStop =
                context.EvaluateStopping(state);

            if (generationStop.ShouldStop)
            {
                return context.Complete(
                    generationStop,
                    state);
            }
        }

        string criterion =
            mode == AdvancedCmaEsMode.ActiveFullCovariance
                ? "MaximumActiveCmaEsGenerations"
                : "MaximumSeparableCmaEsGenerations";

        string message =
            mode == AdvancedCmaEsMode.ActiveFullCovariance
                ? "The configured Active CMA-ES generation limit was reached."
                : "The configured sep-CMA-ES generation limit was reached.";

        return context.Complete(
            StoppingDecision.Stop(
                criterion,
                message),
            state);
    }

    private static void UpdateActiveFullCovariance(
        double[] covariance,
        double[] eigenvectors,
        double[] axisScales,
        double[][] normalizedSteps,
        int[] order,
        double[] activeWeights,
        double[] activeStepScale,
        double[] negativeWhitened,
        int dimension,
        int parentCount,
        double c1,
        double cMu,
        double cC,
        bool hSigma,
        double[] pC)
    {
        double weightSum = 0.0;

        for (int rank = 0; rank < activeWeights.Length; rank++)
        {
            weightSum += activeWeights[rank];
            activeStepScale[rank] = 1.0;
        }

        for (int rank = parentCount;
             rank < activeWeights.Length;
             rank++)
        {
            int index = order[rank];

            CmaEsSymmetricEigenSolver.ApplyInverseSquareRoot(
                eigenvectors,
                axisScales,
                normalizedSteps[index],
                negativeWhitened);

            double normSquared = 0.0;

            for (int i = 0; i < dimension; i++)
            {
                normSquared +=
                    negativeWhitened[i] *
                    negativeWhitened[i];
            }

            activeStepScale[rank] =
                dimension /
                Math.Max(
                    double.Epsilon,
                    normSquared);
        }

        double oldCovarianceFactor =
            1.0 -
            c1 -
            (cMu * weightSum) +
            (hSigma
                ? 0.0
                : c1 *
                  cC *
                  (2.0 - cC));

        for (int row = 0; row < dimension; row++)
        {
            for (int column = 0; column <= row; column++)
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

                double rankUpdate = 0.0;

                for (int rank = 0;
                     rank < activeWeights.Length;
                     rank++)
                {
                    int index = order[rank];
                    double[] step = normalizedSteps[index];

                    rankUpdate +=
                        activeWeights[rank] *
                        activeStepScale[rank] *
                        step[row] *
                        step[column];
                }

                updated +=
                    cMu *
                    rankUpdate;

                if (!double.IsFinite(updated))
                {
                    throw new InvalidOperationException(
                        "Active CMA-ES covariance update became non-finite.");
                }

                covariance[rc] =
                    updated;

                covariance[
                    (column * dimension) +
                    row] =
                    updated;
            }
        }

    }

    private static double UpdateSeparableCovariance(
        double[] variances,
        double[] axisScales,
        double[][] normalizedSteps,
        int[] order,
        double[] positiveWeights,
        int dimension,
        double muEffective,
        double cCov,
        double cC,
        bool hSigma,
        double[] pC,
        double minimumEigenvalue)
    {
        double minVariance =
            double.PositiveInfinity;

        double maxVariance =
            0.0;

        for (int coordinate = 0;
             coordinate < dimension;
             coordinate++)
        {
            double rankMu = 0.0;

            for (int rank = 0;
                 rank < positiveWeights.Length;
                 rank++)
            {
                double step =
                    normalizedSteps[
                        order[rank]][coordinate];

                rankMu +=
                    positiveWeights[rank] *
                    step *
                    step;
            }

            double retention =
                1.0 -
                cCov +
                (hSigma
                    ? 0.0
                    : (cCov / muEffective) *
                      cC *
                      (2.0 - cC));

            double updated =
                (retention *
                 variances[coordinate]) +
                ((cCov / muEffective) *
                 pC[coordinate] *
                 pC[coordinate]) +
                (cCov *
                 (1.0 - (1.0 / muEffective)) *
                 rankMu);

            if (!double.IsFinite(updated))
            {
                throw new InvalidOperationException(
                    "sep-CMA-ES diagonal covariance update became non-finite.");
            }

            updated =
                Math.Max(
                    minimumEigenvalue,
                    updated);

            variances[coordinate] =
                updated;

            axisScales[coordinate] =
                Math.Sqrt(updated);

            minVariance =
                Math.Min(
                    minVariance,
                    updated);

            maxVariance =
                Math.Max(
                    maxVariance,
                    updated);
        }

        return
            maxVariance /
            minVariance;
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

            weights[rank] = weight;
            sum += weight;
        }

        for (int rank = 0;
             rank < parentCount;
             rank++)
        {
            weights[rank] /= sum;
        }

        return weights;
    }

    private static double[] BuildActiveWeights(
        int populationSize,
        int parentCount,
        double[] positiveWeights,
        double dimension,
        double muEffectivePositive,
        double c1,
        double cMu)
    {
        if (cMu <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parentCount),
                "Active CMA-ES requires a parent configuration with positive rank-mu learning rate.");
        }

        double[] weights =
            new double[populationSize];

        for (int i = 0; i < parentCount; i++)
        {
            weights[i] = positiveWeights[i];
        }

        double negativeAbsoluteSum = 0.0;
        double negativeSquaredSum = 0.0;

        double zeroCrossing =
            (populationSize + 1.0) /
            2.0;

        for (int rank = parentCount;
             rank < populationSize;
             rank++)
        {
            double rawWeight =
                Math.Log(zeroCrossing) -
                Math.Log(rank + 1.0);

            if (rawWeight >= 0.0)
            {
                // A custom parent count may stop before the canonical
                // positive-weight boundary. Such omitted positive ranks
                // contribute zero rather than being reinterpreted as
                // negative covariance information.
                weights[rank] = 0.0;
                continue;
            }

            double magnitude =
                -rawWeight;

            weights[rank] =
                rawWeight;

            negativeAbsoluteSum += magnitude;
            negativeSquaredSum +=
                magnitude *
                magnitude;
        }

        if (negativeAbsoluteSum <= 0.0 ||
            negativeSquaredSum <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parentCount),
                "Active CMA-ES requires at least one strictly negative ranked covariance weight.");
        }

        double muEffectiveNegative =
            (negativeAbsoluteSum *
             negativeAbsoluteSum) /
            negativeSquaredSum;

        double alphaMu =
            1.0 +
            (c1 / cMu);

        double alphaMuEffective =
            1.0 +
            ((2.0 * muEffectiveNegative) /
             (muEffectivePositive + 2.0));

        double alphaPositiveDefinite =
            (1.0 - c1 - cMu) /
            (dimension * cMu);

        double negativeMass =
            Math.Max(
                0.0,
                Math.Min(
                    alphaMu,
                    Math.Min(
                        alphaMuEffective,
                        alphaPositiveDefinite)));

        for (int rank = parentCount;
             rank < populationSize;
             rank++)
        {
            weights[rank] =
                (weights[rank] /
                 negativeAbsoluteSum) *
                negativeMass;
        }

        return weights;
    }

    private static double ResolveSeparableCovarianceLearningRate(
        double dimension,
        double muEffective)
    {
        double defaultRate =
            (1.0 / muEffective) *
            (2.0 /
             Math.Pow(
                 dimension + Math.Sqrt(2.0),
                 2.0));

        defaultRate +=
            (1.0 - (1.0 / muEffective)) *
            Math.Min(
                1.0,
                ((2.0 * muEffective) - 1.0) /
                (Math.Pow(dimension + 2.0, 2.0) +
                 muEffective));

        return
            Math.Min(
                1.0,
                ((dimension + 2.0) / 3.0) *
                defaultRate);
    }

    private static double[] ResolveInitialMean(
        IBoundedContinuousSearchSpace searchSpace,
        CmaEsParameters parameters)
    {
        int dimension = searchSpace.Dimension;

        if (parameters.InitialMean is not null)
        {
            if (parameters.InitialMean.Length != dimension)
            {
                throw new ArgumentException(
                    "InitialMean dimension does not match the search space.",
                    nameof(parameters));
            }

            double[] mean =
                (double[])parameters.InitialMean.Clone();

            if (!searchSpace.Contains(mean))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(parameters),
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

    private static double ResolveInitialStepSize(
        IBoundedContinuousSearchSpace searchSpace,
        CmaEsParameters parameters)
    {
        if (parameters.InitialStepSize.HasValue)
        {
            return parameters.InitialStepSize.Value;
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
                "Unable to derive a valid default Advanced CMA-ES initial step size.");
        }

        return sigma;
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
