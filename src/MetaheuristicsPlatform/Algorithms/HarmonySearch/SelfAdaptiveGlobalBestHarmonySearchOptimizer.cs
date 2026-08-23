using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Self-Adaptive Global-best Harmony Search following
/// Pan, Suganthan, Tasgetiren and Liang (2010).
/// </summary>
public sealed class SelfAdaptiveGlobalBestHarmonySearchOptimizer :
    IMetaheuristic<double[], SelfAdaptiveGlobalBestHarmonySearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch,
            Name = "Self-Adaptive Global-best Harmony Search",
            Acronym = "SGHS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms =
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                HarmonySearchReferences.PanSuganthanTasgetirenLiang2010
            ]
        };

    public SelfAdaptiveGlobalBestHarmonySearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        SelfAdaptiveGlobalBestHarmonySearchParameters parameters,
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
                "Self-Adaptive Global-best Harmony Search requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Self-Adaptive Global-best Harmony Search requires a positive search-space dimension.");
        }

        double[] coordinateSpans =
            new double[dimension];

        double minimumMaximumBandwidth =
            double.PositiveInfinity;

        double maximumMaximumBandwidth =
            double.NegativeInfinity;

        for (int coordinate = 0;
             coordinate < dimension;
             coordinate++)
        {
            double span =
                searchSpace.UpperBounds[coordinate] -
                searchSpace.LowerBounds[coordinate];

            if (!double.IsFinite(span) ||
                span <= 0.0)
            {
                throw new InvalidOperationException(
                    "SGHS requires every continuous coordinate to have a finite positive span.");
            }

            coordinateSpans[coordinate] =
                span;

            double maximumBandwidth =
                parameters.MaximumPitchAdjustmentBandwidthFractionOfRange *
                span;

            if (maximumBandwidth <
                parameters.MinimumPitchAdjustmentBandwidth)
            {
                throw new InvalidOperationException(
                    "SGHS BWmin exceeds the coordinate-wise BWmax=(UB-LB)*fraction. " +
                    "Adjust the SGHS bandwidth parameters for this search space.");
            }

            minimumMaximumBandwidth =
                Math.Min(
                    minimumMaximumBandwidth,
                    maximumBandwidth);

            maximumMaximumBandwidth =
                Math.Max(
                    maximumMaximumBandwidth,
                    maximumBandwidth);
        }

        int harmonyMemorySize =
            parameters.HarmonyMemorySize;

        double[][] harmonyMemory =
            new double[harmonyMemorySize][];

        double[] objectiveValues =
            new double[harmonyMemorySize];

        for (int i = 0; i < harmonyMemorySize; i++)
        {
            harmonyMemory[i] =
                new double[dimension];
        }

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        double meanHmcr =
            parameters.InitialMeanHarmonyMemoryConsiderationRate;

        double meanPar =
            parameters.InitialMeanPitchAdjustmentRate;

        var successfulHmcr =
            new List<double>(
                parameters.LearningPeriod);

        var successfulPar =
            new List<double>(
                parameters.LearningPeriod);

        int learningPeriodPosition = 0;
        int learningUpdates = 0;
        int lastCompletedLearningPeriodSuccessfulSamples = 0;

        SelfAdaptiveGlobalBestHarmonySearchState state =
            new(
                Iteration: 0,
                Phase: HarmonySearchPhase.Initialization,
                HarmonyMemorySize: harmonyMemorySize,
                TotalImprovisations: 0,
                ReplacedWorstHarmony: false,
                HarmonyMemoryConsiderationRate: meanHmcr,
                PitchAdjustmentRate: meanPar,
                MeanHarmonyMemoryConsiderationRate: meanHmcr,
                MeanPitchAdjustmentRate: meanPar,
                LearningPeriodPosition: 0,
                SuccessfulSamplesInCurrentLearningPeriod: 0,
                LearningUpdates: 0,
                LastCompletedLearningPeriodSuccessfulSamples: 0,
                MinimumCurrentBandwidth:
                    parameters.MinimumPitchAdjustmentBandwidth,
                MaximumCurrentBandwidth:
                    maximumMaximumBandwidth,
                MemoryBestFitness: null,
                MemoryWorstFitness: null);

        context.Start(state);

        for (int i = 0; i < harmonyMemorySize; i++)
        {
            searchSpace.Sample(
                context.Random,
                harmonyMemory[i]);

            objectiveValues[i] =
                context.Evaluate(
                    harmonyMemory[i],
                    state);

            RequireFiniteObjective(
                objectiveValues[i]);

            StoppingDecision initializationStop =
                context.EvaluateStopping(
                    state);

            if (initializationStop.ShouldStop)
            {
                return context.Complete(
                    initializationStop,
                    state);
            }
        }

        double[] improvisedHarmony =
            new double[dimension];

        for (int improvisation = 1;
             improvisation <= parameters.MaximumImprovisations;
             improvisation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double hmcr =
                SampleTruncatedNormal(
                    context.Random,
                    meanHmcr,
                    SelfAdaptiveGlobalBestHarmonySearchParameters
                        .HarmonyMemoryConsiderationRateStandardDeviation,
                    SelfAdaptiveGlobalBestHarmonySearchParameters
                        .HarmonyMemoryConsiderationRateMinimum,
                    SelfAdaptiveGlobalBestHarmonySearchParameters
                        .HarmonyMemoryConsiderationRateMaximum);

            double par =
                SampleTruncatedNormal(
                    context.Random,
                    meanPar,
                    SelfAdaptiveGlobalBestHarmonySearchParameters
                        .PitchAdjustmentRateStandardDeviation,
                    SelfAdaptiveGlobalBestHarmonySearchParameters
                        .PitchAdjustmentRateMinimum,
                    SelfAdaptiveGlobalBestHarmonySearchParameters
                        .PitchAdjustmentRateMaximum);

            int bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            int currentWorst =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            double currentMinimumBandwidth =
                double.PositiveInfinity;

            double currentMaximumBandwidth =
                double.NegativeInfinity;

            for (int coordinate = 0;
                 coordinate < dimension;
                 coordinate++)
            {
                double bandwidth =
                    parameters.GetPitchAdjustmentBandwidth(
                        improvisation,
                        coordinateSpans[coordinate]);

                currentMinimumBandwidth =
                    Math.Min(
                        currentMinimumBandwidth,
                        bandwidth);

                currentMaximumBandwidth =
                    Math.Max(
                        currentMaximumBandwidth,
                        bandwidth);
            }

            state =
                new SelfAdaptiveGlobalBestHarmonySearchState(
                    Iteration: improvisation - 1,
                    Phase: HarmonySearchPhase.Improvisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation - 1,
                    ReplacedWorstHarmony: false,
                    HarmonyMemoryConsiderationRate: hmcr,
                    PitchAdjustmentRate: par,
                    MeanHarmonyMemoryConsiderationRate: meanHmcr,
                    MeanPitchAdjustmentRate: meanPar,
                    LearningPeriodPosition: learningPeriodPosition,
                    SuccessfulSamplesInCurrentLearningPeriod:
                        successfulHmcr.Count,
                    LearningUpdates: learningUpdates,
                    LastCompletedLearningPeriodSuccessfulSamples:
                        lastCompletedLearningPeriodSuccessfulSamples,
                    MinimumCurrentBandwidth:
                        currentMinimumBandwidth,
                    MaximumCurrentBandwidth:
                        currentMaximumBandwidth,
                    MemoryBestFitness:
                        objectiveValues[bestIndex],
                    MemoryWorstFitness:
                        objectiveValues[currentWorst]);

            ImproviseHarmony(
                harmonyMemory,
                bestIndex,
                improvisedHarmony,
                hmcr,
                par,
                improvisation,
                coordinateSpans,
                parameters,
                searchSpace,
                context.Random);

            double candidateObjective =
                context.Evaluate(
                    improvisedHarmony,
                    state);

            RequireFiniteObjective(
                candidateObjective);

            currentWorst =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            bool replacedWorst =
                problem.Sense.IsBetter(
                    candidateObjective,
                    objectiveValues[currentWorst]);

            if (replacedWorst)
            {
                improvisedHarmony.AsSpan().CopyTo(
                    harmonyMemory[currentWorst]);

                objectiveValues[currentWorst] =
                    candidateObjective;

                successfulHmcr.Add(
                    hmcr);

                successfulPar.Add(
                    par);
            }

            learningPeriodPosition++;

            if (learningPeriodPosition ==
                parameters.LearningPeriod)
            {
                lastCompletedLearningPeriodSuccessfulSamples =
                    successfulHmcr.Count;

                if (successfulHmcr.Count > 0)
                {
                    meanHmcr =
                        Average(
                            successfulHmcr);

                    meanPar =
                        Average(
                            successfulPar);
                }

                successfulHmcr.Clear();
                successfulPar.Clear();
                learningPeriodPosition = 0;
                learningUpdates++;
            }

            bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            int worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new SelfAdaptiveGlobalBestHarmonySearchState(
                    Iteration: improvisation,
                    Phase: HarmonySearchPhase.CompletedImprovisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation,
                    ReplacedWorstHarmony: replacedWorst,
                    HarmonyMemoryConsiderationRate: hmcr,
                    PitchAdjustmentRate: par,
                    MeanHarmonyMemoryConsiderationRate: meanHmcr,
                    MeanPitchAdjustmentRate: meanPar,
                    LearningPeriodPosition: learningPeriodPosition,
                    SuccessfulSamplesInCurrentLearningPeriod:
                        successfulHmcr.Count,
                    LearningUpdates: learningUpdates,
                    LastCompletedLearningPeriodSuccessfulSamples:
                        lastCompletedLearningPeriodSuccessfulSamples,
                    MinimumCurrentBandwidth:
                        currentMinimumBandwidth,
                    MaximumCurrentBandwidth:
                        currentMaximumBandwidth,
                    MemoryBestFitness:
                        objectiveValues[bestIndex],
                    MemoryWorstFitness:
                        objectiveValues[worstIndex]);

            context.CompleteIteration(
                objectiveValues[bestIndex],
                state);

            StoppingDecision stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumSelfAdaptiveGlobalBestHarmonySearchImprovisations",
                "The configured Self-Adaptive Global-best Harmony Search improvisation limit was reached."),
            state);
    }

    private static void ImproviseHarmony(
        double[][] harmonyMemory,
        int bestIndex,
        Span<double> destination,
        double harmonyMemoryConsiderationRate,
        double pitchAdjustmentRate,
        int generation,
        ReadOnlySpan<double> coordinateSpans,
        SelfAdaptiveGlobalBestHarmonySearchParameters parameters,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random)
    {
        ReadOnlySpan<double> lowerBounds =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upperBounds =
            searchSpace.UpperBounds;

        for (int coordinate = 0;
             coordinate < destination.Length;
             coordinate++)
        {
            bool memoryConsideration =
                random.NextDouble() <
                harmonyMemoryConsiderationRate;

            double value;

            if (memoryConsideration)
            {
                int sourceHarmony =
                    random.NextInt32(
                        harmonyMemory.Length);

                value =
                    harmonyMemory[sourceHarmony][coordinate];

                double bandwidth =
                    parameters.GetPitchAdjustmentBandwidth(
                        generation,
                        coordinateSpans[coordinate]);

                double direction =
                    random.NextDouble() < 0.5
                        ? -1.0
                        : 1.0;

                value +=
                    direction *
                    random.NextDouble() *
                    bandwidth;

                if (random.NextDouble() <
                    pitchAdjustmentRate)
                {
                    value =
                        harmonyMemory[bestIndex][coordinate];
                }
            }
            else
            {
                double lower =
                    lowerBounds[coordinate];

                double upper =
                    upperBounds[coordinate];

                value =
                    lower +
                    (random.NextDouble() *
                     (upper - lower));
            }

            if (!double.IsFinite(value))
            {
                throw new InvalidOperationException(
                    "Self-Adaptive Global-best Harmony Search produced a non-finite coordinate.");
            }

            destination[coordinate] =
                value;
        }

        // The literature defines SGHS for bounded continuous test problems.
        // Final clamping is the platform's explicit boundary-repair policy.
        searchSpace.Clamp(
            destination);
    }

    private static double SampleTruncatedNormal(
        IRandomSource random,
        double mean,
        double standardDeviation,
        double minimum,
        double maximum)
    {
        while (true)
        {
            double firstUniform;

            do
            {
                firstUniform =
                    random.NextDouble();
            }
            while (firstUniform <= 0.0);

            double secondUniform =
                random.NextDouble();

            double standardNormal =
                Math.Sqrt(
                    -2.0 *
                    Math.Log(
                        firstUniform)) *
                Math.Cos(
                    2.0 *
                    Math.PI *
                    secondUniform);

            double sample =
                mean +
                (standardDeviation *
                 standardNormal);

            if (sample >= minimum &&
                sample <= maximum)
            {
                return sample;
            }
        }
    }

    private static double Average(
        List<double> values)
    {
        double sum = 0.0;

        for (int i = 0;
             i < values.Count;
             i++)
        {
            sum +=
                values[i];
        }

        return
            sum /
            values.Count;
    }

    private static int FindBestIndex(
        ReadOnlySpan<double> objectiveValues,
        OptimizationSense sense)
    {
        int bestIndex = 0;

        for (int i = 1; i < objectiveValues.Length; i++)
        {
            if (sense.IsBetter(
                objectiveValues[i],
                objectiveValues[bestIndex]))
            {
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private static int FindWorstIndex(
        ReadOnlySpan<double> objectiveValues,
        OptimizationSense sense)
    {
        int worstIndex = 0;

        for (int i = 1; i < objectiveValues.Length; i++)
        {
            if (sense.IsBetter(
                objectiveValues[worstIndex],
                objectiveValues[i]))
            {
                worstIndex = i;
            }
        }

        return worstIndex;
    }

    private static void RequireFiniteObjective(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Self-Adaptive Global-best Harmony Search requires finite objective values.");
        }
    }
}
