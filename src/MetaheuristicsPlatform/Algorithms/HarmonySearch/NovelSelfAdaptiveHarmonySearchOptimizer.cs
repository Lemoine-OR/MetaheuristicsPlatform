using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class NovelSelfAdaptiveHarmonySearchOptimizer :
    IMetaheuristic<double[], NovelSelfAdaptiveHarmonySearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.NovelSelfAdaptiveHarmonySearch,
            Name = "Novel Self-Adaptive Harmony Search",
            Acronym = "NSHS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.MemoryBased | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [ HarmonySearchReferences.Luo2013NovelSelfAdaptive ]
        };

    public NovelSelfAdaptiveHarmonySearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        NovelSelfAdaptiveHarmonySearchParameters parameters,
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

        if (problem.Sense != OptimizationSense.Minimize)
        {
            throw new NotSupportedException(
                "This published Harmony Search variant is implemented for minimization only.");
        }

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "Novel Self-Adaptive Harmony Search requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
        {
            throw new InvalidOperationException("Positive dimension required.");
        }

        int harmonyMemorySize = parameters.HarmonyMemorySize;
        double[][] harmonyMemory = new double[harmonyMemorySize][];
        double[] objectiveValues = new double[harmonyMemorySize];
        for (int i = 0; i < harmonyMemorySize; i++)
        {
            harmonyMemory[i] = new double[dimension];
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
                HarmonyMemoryConsiderationRate: parameters.HarmonyMemoryConsiderationRate,
                PitchAdjustmentRate: parameters.ReportedPitchAdjustmentRate,
                PitchAdjustmentBandwidth: parameters.ReportedPitchAdjustmentBandwidth,
                MemoryBestFitness: null,
                MemoryWorstFitness: null);

        context.Start(state);

        for (int i = 0; i < harmonyMemorySize; i++)
        {
            searchSpace.Sample(context.Random, harmonyMemory[i]);
            objectiveValues[i] = context.Evaluate(harmonyMemory[i], state);
            RequireFiniteObjective(objectiveValues[i]);

            StoppingDecision initializationStop = context.EvaluateStopping(state);
            if (initializationStop.ShouldStop)
            {
                return context.Complete(initializationStop, state);
            }
        }

        double[] candidate = new double[dimension];

        for (int improvisation = 1;
             improvisation <= parameters.MaximumImprovisations;
             improvisation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int worstIndex = FindWorstIndex(objectiveValues, problem.Sense);
            int bestIndex = FindBestIndex(objectiveValues, problem.Sense);

            state =
                new HarmonySearchState(
                    Iteration: improvisation - 1,
                    Phase: HarmonySearchPhase.Improvisation,
                    HarmonyMemorySize: harmonyMemory.Length,
                    TotalImprovisations: improvisation - 1,
                    ReplacedWorstHarmony: false,
                    HarmonyMemoryConsiderationRate: parameters.HarmonyMemoryConsiderationRate,
                    PitchAdjustmentRate: parameters.ReportedPitchAdjustmentRate,
                    PitchAdjustmentBandwidth: parameters.ReportedPitchAdjustmentBandwidth,
                    MemoryBestFitness: objectiveValues[bestIndex],
                    MemoryWorstFitness: objectiveValues[worstIndex]);

            ImproviseHarmony(
                harmonyMemory,
                objectiveValues,
                candidate,
                parameters,
                searchSpace,
                context.Random,
                improvisation);

            double candidateObjective = context.Evaluate(candidate, state);
            RequireFiniteObjective(candidateObjective);

            worstIndex = FindWorstIndex(objectiveValues, problem.Sense);
            double replacedObjective = objectiveValues[worstIndex];
            bool replacedWorst =
                problem.Sense.IsBetter(candidateObjective, replacedObjective);

            if (replacedWorst)
            {
                candidate.AsSpan().CopyTo(harmonyMemory[worstIndex]);
                objectiveValues[worstIndex] = candidateObjective;
            }

            bestIndex = FindBestIndex(objectiveValues, problem.Sense);
            worstIndex = FindWorstIndex(objectiveValues, problem.Sense);

            state =
                new HarmonySearchState(
                    Iteration: improvisation,
                    Phase: HarmonySearchPhase.CompletedImprovisation,
                    HarmonyMemorySize: harmonyMemory.Length,
                    TotalImprovisations: improvisation,
                    ReplacedWorstHarmony: replacedWorst,
                    HarmonyMemoryConsiderationRate: parameters.HarmonyMemoryConsiderationRate,
                    PitchAdjustmentRate: parameters.ReportedPitchAdjustmentRate,
                    PitchAdjustmentBandwidth: parameters.ReportedPitchAdjustmentBandwidth,
                    MemoryBestFitness: objectiveValues[bestIndex],
                    MemoryWorstFitness: objectiveValues[worstIndex]);

            context.CompleteIteration(objectiveValues[bestIndex], state);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumNovelSelfAdaptiveHarmonySearchImprovisations",
                "The configured scientific Harmony Search improvisation limit was reached."),
            state);
    }


    private static void ImproviseHarmony(
        double[][] harmonyMemory,
        double[] objectiveValues,
        Span<double> destination,
        NovelSelfAdaptiveHarmonySearchParameters parameters,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random,
        int improvisation)
    {
        ReadOnlySpan<double> lower = searchSpace.LowerBounds;
        ReadOnlySpan<double> upper = searchSpace.UpperBounds;

        int bestIndex = FindBestIndex(
            objectiveValues,
            OptimizationSense.Minimize);
        int worstIndex = FindWorstIndex(
            objectiveValues,
            OptimizationSense.Minimize);

        double mean = 0.0;
        for (int i = 0; i < objectiveValues.Length; i++)
        {
            mean += objectiveValues[i];
        }
        mean /= objectiveValues.Length;

        double variance = 0.0;
        for (int i = 0; i < objectiveValues.Length; i++)
        {
            double delta = objectiveValues[i] - mean;
            variance += delta * delta;
        }
        variance /= objectiveValues.Length;
        double fstd = Math.Sqrt(variance);

        double hmcr =
            NovelSelfAdaptiveHarmonySearchParameters
                .GetHarmonyMemoryConsiderationRate(destination.Length);

        for (int coordinate = 0; coordinate < destination.Length; coordinate++)
        {
            if (random.NextDouble() < hmcr)
            {
                destination[coordinate] =
                    harmonyMemory[random.NextInt32(harmonyMemory.Length)][coordinate];
            }
            else if (fstd > parameters.FitnessStandardDeviationThreshold)
            {
                destination[coordinate] =
                    lower[coordinate] +
                    random.NextDouble() *
                    (upper[coordinate] - lower[coordinate]);
            }
            else
            {
                double best = harmonyMemory[bestIndex][coordinate];
                double worst = harmonyMemory[worstIndex][coordinate];
                destination[coordinate] =
                    best + random.NextDouble() * (worst - best);
            }

            double perturbation;
            if (fstd > parameters.FitnessStandardDeviationThreshold)
            {
                double schedule =
                    1.0 -
                    ((double)improvisation /
                     parameters.MaximumImprovisations);

                perturbation =
                    ((upper[coordinate] - lower[coordinate]) / 100.0) *
                    schedule *
                    ((2.0 * random.NextDouble()) - 1.0);
            }
            else
            {
                perturbation =
                    parameters.FitnessStandardDeviationThreshold *
                    ((2.0 * random.NextDouble()) - 1.0);
            }

            destination[coordinate] += perturbation;
        }

        searchSpace.Clamp(destination);
    }


    private static int FindBestIndex(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int index = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (sense.IsBetter(values[i], values[index]))
            {
                index = i;
            }
        }
        return index;
    }

    private static int FindWorstIndex(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int index = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (sense.IsBetter(values[index], values[i]))
            {
                index = i;
            }
        }
        return index;
    }

    private static void RequireFiniteObjective(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "Harmony Search variant requires finite objective values.");
        }
    }

}
