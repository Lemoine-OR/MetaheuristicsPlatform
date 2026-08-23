using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Canonical bounded-continuous Harmony Search foundation following
/// Geem, Kim and Loganathan (2001).
/// </summary>
public sealed class HarmonySearchOptimizer :
    IMetaheuristic<double[], HarmonySearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.HarmonySearch,
            Name = "Harmony Search",
            Acronym = "HS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.MemoryBased,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                HarmonySearchReferences.GeemKimLoganathan2001
            ]
        };

    public HarmonySearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        HarmonySearchParameters parameters,
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
                "Harmony Search requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Harmony Search requires a positive search-space dimension.");
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

        HarmonySearchState state =
            new(
                Iteration: 0,
                Phase: HarmonySearchPhase.Initialization,
                HarmonyMemorySize: harmonyMemorySize,
                TotalImprovisations: 0,
                ReplacedWorstHarmony: false,
                HarmonyMemoryConsiderationRate:
                    parameters.HarmonyMemoryConsiderationRate,
                PitchAdjustmentRate:
                    parameters.PitchAdjustmentRate,
                PitchAdjustmentBandwidth:
                    parameters.PitchAdjustmentBandwidth,
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

            int currentWorst =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new HarmonySearchState(
                    Iteration: improvisation - 1,
                    Phase: HarmonySearchPhase.Improvisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation - 1,
                    ReplacedWorstHarmony: false,
                    HarmonyMemoryConsiderationRate:
                        parameters.HarmonyMemoryConsiderationRate,
                    PitchAdjustmentRate:
                        parameters.PitchAdjustmentRate,
                    PitchAdjustmentBandwidth:
                        parameters.PitchAdjustmentBandwidth,
                    MemoryBestFitness:
                        FindBestObjective(
                            objectiveValues,
                            problem.Sense),
                    MemoryWorstFitness:
                        objectiveValues[currentWorst]);

            ImproviseHarmony(
                harmonyMemory,
                improvisedHarmony,
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
            }

            int bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            int worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new HarmonySearchState(
                    Iteration: improvisation,
                    Phase: HarmonySearchPhase.CompletedImprovisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation,
                    ReplacedWorstHarmony: replacedWorst,
                    HarmonyMemoryConsiderationRate:
                        parameters.HarmonyMemoryConsiderationRate,
                    PitchAdjustmentRate:
                        parameters.PitchAdjustmentRate,
                    PitchAdjustmentBandwidth:
                        parameters.PitchAdjustmentBandwidth,
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
                "MaximumHarmonySearchImprovisations",
                "The configured Harmony Search improvisation limit was reached."),
            state);
    }

    private static void ImproviseHarmony(
        double[][] harmonyMemory,
        Span<double> destination,
        HarmonySearchParameters parameters,
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
                parameters.HarmonyMemoryConsiderationRate;

            double value;

            if (memoryConsideration)
            {
                int sourceHarmony =
                    random.NextInt32(
                        harmonyMemory.Length);

                value =
                    harmonyMemory[sourceHarmony][coordinate];

                bool pitchAdjustment =
                    random.NextDouble() <
                    parameters.PitchAdjustmentRate;

                if (pitchAdjustment)
                {
                    double direction =
                        random.NextDouble() < 0.5
                            ? -1.0
                            : 1.0;

                    double distance =
                        random.NextDouble() *
                        parameters.PitchAdjustmentBandwidth;

                    value +=
                        direction *
                        distance;
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
                    "Harmony Search produced a non-finite improvised coordinate.");
            }

            destination[coordinate] =
                value;
        }

        searchSpace.Clamp(
            destination);
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

    private static double FindBestObjective(
        ReadOnlySpan<double> objectiveValues,
        OptimizationSense sense) =>
        objectiveValues[
            FindBestIndex(
                objectiveValues,
                sense)];

    private static void RequireFiniteObjective(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Harmony Search requires finite objective values.");
        }
    }
}