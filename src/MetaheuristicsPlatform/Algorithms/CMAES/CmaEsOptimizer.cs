using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// Canonical full-covariance CMA-ES with positive logarithmic recombination,
/// rank-one/rank-mu covariance adaptation and cumulative step-size adaptation.
/// </summary>
public sealed class CmaEsOptimizer :
    IMetaheuristic<double[], CmaEsParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CmaEs,
            Name = "Covariance Matrix Adaptation Evolution Strategy",
            Acronym = "CMA-ES",
            SolutionModel =
                MetaheuristicSolutionModel.Population,
            Families =
                MetaheuristicFamily.Evolutionary,
            Mechanisms =
                MetaheuristicMechanism.EvolutionaryOperators |
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.MemoryBased,
            SearchSpaces =
                SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                CmaEsReferences.HansenOstermeier2001,
                CmaEsReferences.HansenMullerKoumoutsakos2003
            ]
        };

    public CmaEsParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        CmaEsParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);

        parameters.Validate();

        if (problem is not
            ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "CMA-ES requires ISpanContinuousOptimizationProblem.");
        }

        int dimension =
            continuousProblem.SearchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "CMA-ES requires a positive search-space dimension.");
        }

        int lambda =
            parameters.PopulationSize > 0
                ? parameters.PopulationSize
                : 4 +
                  (int)Math.Floor(
                      3.0 *
                      Math.Log(dimension));

        lambda =
            Math.Max(2, lambda);

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
            ResolveInitialMean(
                continuousProblem.SearchSpace,
                parameters);

        double sigma =
            ResolveInitialStepSize(
                continuousProblem.SearchSpace,
                parameters);

        double[] covariance =
            new double[dimension * dimension];

        double[] eigenvectors =
            new double[dimension * dimension];

        double[] axisScales =
            new double[dimension];

        for (int i = 0; i < dimension; i++)
        {
            covariance[(i * dimension) + i] = 1.0;
            eigenvectors[(i * dimension) + i] = 1.0;
            axisScales[i] = 1.0;
        }

        double conditionNumber = 1.0;

        double[] pSigma =
            new double[dimension];

        double[] pC =
            new double[dimension];

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

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
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
                        "CMA-ES requires finite objective values.");
                }

                fitness[k] =
                    objective;

                if (!generationBest.HasValue ||
                    problem.Sense.IsBetter(
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
                    // No covariance/mean update from a partial generation.
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
                            "CMA-ES covariance update became non-finite.");
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
                    "CMA-ES step-size adaptation produced an invalid sigma.");
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
                    "CMA-ES covariance condition estimate is invalid.");
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

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumCmaEsGenerations",
                "The configured CMA-ES generation limit was reached."),
            state);
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

    private static double[] ResolveInitialMean(
        IBoundedContinuousSearchSpace searchSpace,
        CmaEsParameters parameters)
    {
        int dimension =
            searchSpace.Dimension;

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
                width * width;
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
                "Unable to derive a valid default CMA-ES initial step size.");
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
