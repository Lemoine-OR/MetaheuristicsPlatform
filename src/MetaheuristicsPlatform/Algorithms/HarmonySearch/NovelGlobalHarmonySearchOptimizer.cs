using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Novel Global Harmony Search following Zou, Gao, Wu and Li (2010).
/// </summary>
/// <remarks>
/// NGHS removes HMCR, PAR and BW. Each coordinate is position-updated from the
/// current worst harmony toward the bounded reflection of the current best harmony,
/// then optionally replaced by uniform genetic mutation. The new harmony replaces
/// the current worst harmony unconditionally, exactly as in the published NGHS.
/// </remarks>
public sealed class NovelGlobalHarmonySearchOptimizer :
    IMetaheuristic<double[], NovelGlobalHarmonySearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.NovelGlobalHarmonySearch,
            Name = "Novel Global Harmony Search",
            Acronym = "NGHS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms =
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                HarmonySearchReferences.ZouGaoWuLi2010NovelGlobal
            ]
        };

    public NovelGlobalHarmonySearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        NovelGlobalHarmonySearchParameters parameters,
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
                "Novel Global Harmony Search requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Novel Global Harmony Search requires a positive search-space dimension.");
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

        NovelGlobalHarmonySearchState state =
            new(
                Iteration: 0,
                Phase: HarmonySearchPhase.Initialization,
                HarmonyMemorySize: harmonyMemorySize,
                TotalImprovisations: 0,
                MutationProbability:
                    parameters.MutationProbability,
                MutatedCoordinateCount: 0,
                UnconditionallyReplacedWorstHarmony: false,
                CandidateWasStrictlyBetterThanReplacedWorst: false,
                CandidateFitness: null,
                ReplacedWorstFitness: null,
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

            int bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            int worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new NovelGlobalHarmonySearchState(
                    Iteration: improvisation - 1,
                    Phase: HarmonySearchPhase.Improvisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation - 1,
                    MutationProbability:
                        parameters.MutationProbability,
                    MutatedCoordinateCount: 0,
                    UnconditionallyReplacedWorstHarmony: false,
                    CandidateWasStrictlyBetterThanReplacedWorst: false,
                    CandidateFitness: null,
                    ReplacedWorstFitness:
                        objectiveValues[worstIndex],
                    MemoryBestFitness:
                        objectiveValues[bestIndex],
                    MemoryWorstFitness:
                        objectiveValues[worstIndex]);

            int mutatedCoordinates =
                ImproviseHarmony(
                    harmonyMemory[bestIndex],
                    harmonyMemory[worstIndex],
                    improvisedHarmony,
                    parameters.MutationProbability,
                    searchSpace,
                    context.Random);

            double candidateObjective =
                context.Evaluate(
                    improvisedHarmony,
                    state);

            RequireFiniteObjective(
                candidateObjective);

            double replacedWorstObjective =
                objectiveValues[worstIndex];

            bool candidateWasStrictlyBetter =
                problem.Sense.IsBetter(
                    candidateObjective,
                    replacedWorstObjective);

            // Canonical NGHS replacement is unconditional: the new harmony
            // replaces the current worst harmony even when it is not better.
            improvisedHarmony.AsSpan().CopyTo(
                harmonyMemory[worstIndex]);

            objectiveValues[worstIndex] =
                candidateObjective;

            bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new NovelGlobalHarmonySearchState(
                    Iteration: improvisation,
                    Phase: HarmonySearchPhase.CompletedImprovisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation,
                    MutationProbability:
                        parameters.MutationProbability,
                    MutatedCoordinateCount:
                        mutatedCoordinates,
                    UnconditionallyReplacedWorstHarmony: true,
                    CandidateWasStrictlyBetterThanReplacedWorst:
                        candidateWasStrictlyBetter,
                    CandidateFitness:
                        candidateObjective,
                    ReplacedWorstFitness:
                        replacedWorstObjective,
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
                "MaximumNovelGlobalHarmonySearchImprovisations",
                "The configured Novel Global Harmony Search improvisation limit was reached."),
            state);
    }

    private static int ImproviseHarmony(
        ReadOnlySpan<double> bestHarmony,
        ReadOnlySpan<double> worstHarmony,
        Span<double> destination,
        double mutationProbability,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random)
    {
        ReadOnlySpan<double> lowerBounds =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upperBounds =
            searchSpace.UpperBounds;

        int mutatedCoordinates = 0;

        for (int coordinate = 0;
             coordinate < destination.Length;
             coordinate++)
        {
            double lower =
                lowerBounds[coordinate];

            double upper =
                upperBounds[coordinate];

            double reflectedBest =
                (2.0 * bestHarmony[coordinate]) -
                worstHarmony[coordinate];

            if (reflectedBest > upper)
            {
                reflectedBest =
                    upper;
            }
            else if (reflectedBest < lower)
            {
                reflectedBest =
                    lower;
            }

            double value =
                worstHarmony[coordinate] +
                (random.NextDouble() *
                 (reflectedBest -
                  worstHarmony[coordinate]));

            if (random.NextDouble() <=
                mutationProbability)
            {
                value =
                    lower +
                    (random.NextDouble() *
                     (upper - lower));

                mutatedCoordinates++;
            }

            if (!double.IsFinite(value))
            {
                throw new InvalidOperationException(
                    "Novel Global Harmony Search produced a non-finite coordinate.");
            }

            destination[coordinate] =
                value;
        }

        return mutatedCoordinates;
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
                "Novel Global Harmony Search requires finite objective values.");
        }
    }
}
