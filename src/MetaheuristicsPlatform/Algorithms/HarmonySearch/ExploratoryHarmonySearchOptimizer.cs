using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class ExploratoryHarmonySearchOptimizer :
    IMetaheuristic<double[], ExploratoryHarmonySearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ExploratoryHarmonySearch,
            Name = "Exploratory Harmony Search",
            Acronym = "EHS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.MemoryBased | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [ HarmonySearchReferences.DasMukhopadhyayRoyAbrahamPanigrahi2011 ]
        };

    public ExploratoryHarmonySearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ExploratoryHarmonySearchParameters parameters,
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
                "Exploratory Harmony Search requires ISpanContinuousOptimizationProblem.");
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
                "MaximumExploratoryHarmonySearchImprovisations",
                "The configured scientific Harmony Search improvisation limit was reached."),
            state);
    }


    private static void ImproviseHarmony(
        double[][] harmonyMemory,
        double[] objectiveValues,
        Span<double> destination,
        ExploratoryHarmonySearchParameters parameters,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random,
        int improvisation)
    {
        _ = objectiveValues;
        _ = improvisation;

        ReadOnlySpan<double> lower = searchSpace.LowerBounds;
        ReadOnlySpan<double> upper = searchSpace.UpperBounds;

        for (int coordinate = 0; coordinate < destination.Length; coordinate++)
        {
            if (random.NextDouble() < parameters.HarmonyMemoryConsiderationRate)
            {
                int source = random.NextInt32(harmonyMemory.Length);
                double value = harmonyMemory[source][coordinate];

                if (random.NextDouble() < parameters.PitchAdjustmentRate)
                {
                    double mean = 0.0;
                    for (int h = 0; h < harmonyMemory.Length; h++)
                    {
                        mean += harmonyMemory[h][coordinate];
                    }
                    mean /= harmonyMemory.Length;

                    double variance = 0.0;
                    for (int h = 0; h < harmonyMemory.Length; h++)
                    {
                        double delta = harmonyMemory[h][coordinate] - mean;
                        variance += delta * delta;
                    }
                    variance /= harmonyMemory.Length;

                    double fw =
                        parameters.StandardDeviationMultiplier *
                        Math.Sqrt(variance);

                    value +=
                        (random.NextDouble() < 0.5 ? -1.0 : 1.0) *
                        random.NextDouble() *
                        fw;
                }

                destination[coordinate] = value;
            }
            else
            {
                destination[coordinate] =
                    lower[coordinate] +
                    random.NextDouble() *
                    (upper[coordinate] - lower[coordinate]);
            }
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
