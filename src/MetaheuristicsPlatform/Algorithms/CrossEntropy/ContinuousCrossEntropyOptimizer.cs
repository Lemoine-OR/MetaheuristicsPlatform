using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CrossEntropy;

/// <summary>
/// Continuous Cross-Entropy Method using an independent normal sampling model,
/// elite maximum-likelihood updates and Kroese-Porotsky-Rubinstein dynamic
/// standard-deviation smoothing.
/// </summary>
public sealed class ContinuousCrossEntropyOptimizer :
    IMetaheuristic<double[], ContinuousCrossEntropyParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ContinuousCrossEntropy,
            Name = "Cross-Entropy Method - Continuous Optimization",
            Acronym = "CE",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms =
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.MemoryBased,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                CrossEntropyReferences.KroesePorotskyRubinstein2006,
                CrossEntropyReferences.Rubinstein1999,
                CrossEntropyReferences.DeBoerKroeseMannorRubinstein2005
            ]
        };

    public ContinuousCrossEntropyParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ContinuousCrossEntropyParameters parameters,
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

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "Continuous Cross-Entropy requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Continuous Cross-Entropy requires a positive dimension.");
        }

        int sampleCount =
            parameters.SampleCount;

        int eliteCount =
            Math.Clamp(
                (int)Math.Ceiling(
                    parameters.EliteFraction *
                    sampleCount),
                1,
                sampleCount - 1);

        double[] mean =
            ResolveInitialMean(
                searchSpace,
                parameters.InitialMean);

        double[] standardDeviation =
            ResolveInitialStandardDeviation(
                searchSpace,
                parameters.InitialStandardDeviationScale,
                parameters.MinimumStandardDeviation);

        double[][] samples =
            new double[sampleCount][];

        double[] objectiveValues =
            new double[sampleCount];

        int[] order =
            new int[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] =
                new double[dimension];

            order[i] = i;
        }

        double[] eliteMean =
            new double[dimension];

        double[] eliteStandardDeviation =
            new double[dimension];

        var gaussian =
            new GaussianSampler();

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        (double minSigma, double maxSigma) =
            GetSigmaRange(
                standardDeviation);

        ContinuousCrossEntropyState state =
            new(
                Iteration: 0,
                Phase: CrossEntropyPhase.Sampling,
                SampleCount: sampleCount,
                EliteCount: eliteCount,
                MeanSmoothing: parameters.MeanSmoothing,
                DynamicStandardDeviationSmoothing:
                    parameters.StandardDeviationSmoothingBase,
                MinimumCoordinateStandardDeviation: minSigma,
                MaximumCoordinateStandardDeviation: maxSigma,
                IterationBestFitness: null);

        context.Start(state);

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            state =
                state with
                {
                    Iteration = iteration - 1,
                    Phase = CrossEntropyPhase.Sampling,
                    IterationBestFitness = null
                };

            double? iterationBest =
                null;

            for (int sampleIndex = 0;
                 sampleIndex < sampleCount;
                 sampleIndex++)
            {
                double[] sample =
                    samples[sampleIndex];

                for (int coordinate = 0;
                     coordinate < dimension;
                     coordinate++)
                {
                    sample[coordinate] =
                        mean[coordinate] +
                        (standardDeviation[coordinate] *
                         gaussian.Next(context.Random));
                }

                searchSpace.Clamp(
                    sample.AsSpan());

                double objective =
                    context.Evaluate(
                        sample,
                        state);

                RequireFinite(
                    objective);

                objectiveValues[sampleIndex] =
                    objective;

                if (!iterationBest.HasValue ||
                    problem.Sense.IsBetter(
                        objective,
                        iterationBest.Value))
                {
                    iterationBest =
                        objective;
                }

                StoppingDecision partialStop =
                    context.EvaluateStopping(
                        state);

                if (partialStop.ShouldStop)
                {
                    // Never update the sampling distribution from a partial
                    // iteration. This preserves exact CE generation semantics.
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
                        objectiveValues[left],
                        objectiveValues[right]));

            Array.Clear(
                eliteMean,
                0,
                eliteMean.Length);

            for (int rank = 0;
                 rank < eliteCount;
                 rank++)
            {
                double[] elite =
                    samples[order[rank]];

                for (int coordinate = 0;
                     coordinate < dimension;
                     coordinate++)
                {
                    eliteMean[coordinate] +=
                        elite[coordinate];
                }
            }

            for (int coordinate = 0;
                 coordinate < dimension;
                 coordinate++)
            {
                eliteMean[coordinate] /=
                    eliteCount;
            }

            Array.Clear(
                eliteStandardDeviation,
                0,
                eliteStandardDeviation.Length);

            for (int rank = 0;
                 rank < eliteCount;
                 rank++)
            {
                double[] elite =
                    samples[order[rank]];

                for (int coordinate = 0;
                     coordinate < dimension;
                     coordinate++)
                {
                    double delta =
                        elite[coordinate] -
                        eliteMean[coordinate];

                    eliteStandardDeviation[coordinate] +=
                        delta *
                        delta;
                }
            }

            for (int coordinate = 0;
                 coordinate < dimension;
                 coordinate++)
            {
                eliteStandardDeviation[coordinate] =
                    Math.Sqrt(
                        eliteStandardDeviation[coordinate] /
                        eliteCount);
            }

            double betaT =
                ResolveDynamicStandardDeviationSmoothing(
                    parameters.StandardDeviationSmoothingBase,
                    parameters.DynamicSmoothingExponent,
                    iteration);

            state =
                state with
                {
                    Phase = CrossEntropyPhase.DistributionUpdate,
                    DynamicStandardDeviationSmoothing = betaT,
                    IterationBestFitness = iterationBest
                };

            for (int coordinate = 0;
                 coordinate < dimension;
                 coordinate++)
            {
                mean[coordinate] =
                    (parameters.MeanSmoothing *
                     eliteMean[coordinate]) +
                    ((1.0 - parameters.MeanSmoothing) *
                     mean[coordinate]);

                double updatedSigma =
                    (betaT *
                     eliteStandardDeviation[coordinate]) +
                    ((1.0 - betaT) *
                     standardDeviation[coordinate]);

                if (!double.IsFinite(updatedSigma) ||
                    updatedSigma < 0.0)
                {
                    throw new InvalidOperationException(
                        "Continuous Cross-Entropy produced an invalid standard deviation.");
                }

                standardDeviation[coordinate] =
                    Math.Max(
                        parameters.MinimumStandardDeviation,
                        updatedSigma);
            }

            (minSigma, maxSigma) =
                GetSigmaRange(
                    standardDeviation);

            state =
                new ContinuousCrossEntropyState(
                    Iteration: iteration,
                    Phase: CrossEntropyPhase.CompletedIteration,
                    SampleCount: sampleCount,
                    EliteCount: eliteCount,
                    MeanSmoothing: parameters.MeanSmoothing,
                    DynamicStandardDeviationSmoothing: betaT,
                    MinimumCoordinateStandardDeviation: minSigma,
                    MaximumCoordinateStandardDeviation: maxSigma,
                    IterationBestFitness: iterationBest);

            context.CompleteIteration(
                iterationBest,
                state);

            StoppingDecision globalStop =
                context.EvaluateStopping(
                    state);

            if (globalStop.ShouldStop)
            {
                return context.Complete(
                    globalStop,
                    state);
            }

            if (maxSigma <=
                parameters.MinimumStandardDeviation *
                (1.0 + 1e-12))
            {
                return context.Complete(
                    StoppingDecision.Stop(
                        "CrossEntropyDistributionConverged",
                        "All coordinate standard deviations reached the configured numerical floor."),
                    state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumCrossEntropyIterations",
                "The configured continuous Cross-Entropy iteration limit was reached."),
            state);
    }

    private static double[] ResolveInitialMean(
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

            double[] result =
                (double[])configured.Clone();

            if (!searchSpace.Contains(result))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(configured),
                    "InitialMean must belong to the bounded search space.");
            }

            return result;
        }

        double[] center =
            new double[dimension];

        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        for (int coordinate = 0;
             coordinate < dimension;
             coordinate++)
        {
            center[coordinate] =
                0.5 *
                (lower[coordinate] +
                 upper[coordinate]);
        }

        return center;
    }

    private static double[] ResolveInitialStandardDeviation(
        IBoundedContinuousSearchSpace searchSpace,
        double scale,
        double minimum)
    {
        double[] result =
            new double[searchSpace.Dimension];

        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        for (int coordinate = 0;
             coordinate < result.Length;
             coordinate++)
        {
            double width =
                upper[coordinate] -
                lower[coordinate];

            if (!double.IsFinite(width) ||
                width < 0.0)
            {
                throw new InvalidOperationException(
                    "Continuous Cross-Entropy requires finite ordered bounds.");
            }

            result[coordinate] =
                Math.Max(
                    minimum,
                    scale *
                    width);
        }

        return result;
    }

    private static double ResolveDynamicStandardDeviationSmoothing(
        double beta,
        double exponent,
        int iteration)
    {
        double t =
            iteration;

        double betaT =
            beta -
            (beta *
             Math.Pow(
                 1.0 - (1.0 / t),
                 exponent));

        if (!double.IsFinite(betaT) ||
            betaT <= 0.0 ||
            betaT > beta)
        {
            throw new InvalidOperationException(
                "Dynamic Cross-Entropy smoothing produced an invalid coefficient.");
        }

        return betaT;
    }

    private static (double Minimum, double Maximum)
        GetSigmaRange(
            ReadOnlySpan<double> standardDeviation)
    {
        double minimum =
            double.PositiveInfinity;

        double maximum =
            0.0;

        for (int i = 0;
             i < standardDeviation.Length;
             i++)
        {
            double value =
                standardDeviation[i];

            minimum =
                Math.Min(
                    minimum,
                    value);

            maximum =
                Math.Max(
                    maximum,
                    value);
        }

        return (minimum, maximum);
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

    private static void RequireFinite(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Continuous Cross-Entropy requires finite objective values.");
        }
    }

    private sealed class GaussianSampler
    {
        private bool _hasSpare;
        private double _spare;

        public double Next(
            IRandomSource random)
        {
            if (_hasSpare)
            {
                _hasSpare = false;
                return _spare;
            }

            double u1 =
                Math.Max(
                    double.Epsilon,
                    random.NextDouble());

            double u2 =
                random.NextDouble();

            if (!double.IsFinite(u1) ||
                !double.IsFinite(u2) ||
                u1 <= 0.0 ||
                u1 > 1.0 ||
                u2 < 0.0 ||
                u2 >= 1.0)
            {
                throw new InvalidOperationException(
                    "The random source returned an invalid uniform variate.");
            }

            double radius =
                Math.Sqrt(
                    -2.0 *
                    Math.Log(u1));

            double angle =
                2.0 *
                Math.PI *
                u2;

            double first =
                radius *
                Math.Cos(angle);

            _spare =
                radius *
                Math.Sin(angle);

            _hasSpare = true;
            return first;
        }
    }
}
