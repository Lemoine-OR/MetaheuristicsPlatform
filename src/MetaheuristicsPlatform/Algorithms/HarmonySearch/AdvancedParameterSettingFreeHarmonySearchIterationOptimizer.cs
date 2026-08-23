using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Iteration-dependent Advanced Parameter-Setting-Free Harmony Search
/// following Jeong, Park, Geem and Sim (2020).
/// </summary>
/// <remarks>
/// This public identity implements only the paper's iteration PSF scheme:
/// HMCR follows Equation (5), PAR follows Equation (8), and no Operation
/// Type Matrix is used. The paper's object-dependent HMCR/bandwidth scheme
/// is deliberately reserved for a separate public identity.
/// </remarks>
public sealed class AdvancedParameterSettingFreeHarmonySearchIterationOptimizer :
    IMetaheuristic<
        double[],
        AdvancedParameterSettingFreeHarmonySearchIterationParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id =
                MetaheuristicAlgorithmIds
                    .AdvancedParameterSettingFreeHarmonySearchIteration,
            Name =
                "Advanced Parameter-Setting-Free Harmony Search - Iteration Scheme",
            Acronym = "APSF-HS-I",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms =
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                HarmonySearchReferences
                    .JeongParkGeemSim2020AdvancedParameterSettingFree
            ]
        };

    public AdvancedParameterSettingFreeHarmonySearchIterationParameters
        CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        AdvancedParameterSettingFreeHarmonySearchIterationParameters parameters,
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
                "Iteration Advanced PSF-HS requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Iteration Advanced PSF-HS requires a positive dimension.");
        }

        ValidateFinitePositiveSpans(
            searchSpace);

        int harmonyMemorySize =
            parameters.HarmonyMemorySize;

        double[][] harmonyMemory =
            new double[harmonyMemorySize][];

        double[] objectiveValues =
            new double[harmonyMemorySize];

        for (int row = 0;
             row < harmonyMemorySize;
             row++)
        {
            harmonyMemory[row] =
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

        GetBandwidthRange(
            searchSpace,
            parameters.PitchAdjustmentBandwidthFractionOfRange,
            out double minimumBandwidth,
            out double maximumBandwidth);

        AdvancedParameterSettingFreeHarmonySearchIterationState state =
            new(
                Iteration: 0,
                Phase: HarmonySearchPhase.Initialization,
                HarmonyMemorySize: harmonyMemorySize,
                TotalImprovisations: 0,
                HarmonyMemoryConsiderationRate: 0.0,
                PitchAdjustmentRate: 0.0,
                MinimumPitchAdjustmentBandwidth: minimumBandwidth,
                MaximumPitchAdjustmentBandwidth: maximumBandwidth,
                ReplacedWorstHarmony: false,
                MemoryBestFitness: null,
                MemoryWorstFitness: null);

        context.Start(
            state);

        for (int row = 0;
             row < harmonyMemorySize;
             row++)
        {
            searchSpace.Sample(
                context.Random,
                harmonyMemory[row]);

            objectiveValues[row] =
                context.Evaluate(
                    harmonyMemory[row],
                    state);

            RequireFiniteObjective(
                objectiveValues[row]);

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

        double[] candidate =
            new double[dimension];

        for (int improvisation = 1;
             improvisation <= parameters.MaximumImprovisations;
             improvisation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double hmcr =
                parameters.GetHarmonyMemoryConsiderationRate(
                    improvisation,
                    dimension);

            double par =
                AdvancedParameterSettingFreeHarmonySearchIterationParameters
                    .GetPitchAdjustmentRate(
                        hmcr,
                        dimension);

            int bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            int worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new AdvancedParameterSettingFreeHarmonySearchIterationState(
                    Iteration: improvisation - 1,
                    Phase: HarmonySearchPhase.Improvisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation - 1,
                    HarmonyMemoryConsiderationRate: hmcr,
                    PitchAdjustmentRate: par,
                    MinimumPitchAdjustmentBandwidth: minimumBandwidth,
                    MaximumPitchAdjustmentBandwidth: maximumBandwidth,
                    ReplacedWorstHarmony: false,
                    MemoryBestFitness: objectiveValues[bestIndex],
                    MemoryWorstFitness: objectiveValues[worstIndex]);

            ImproviseHarmony(
                harmonyMemory,
                candidate,
                hmcr,
                par,
                parameters.PitchAdjustmentBandwidthFractionOfRange,
                searchSpace,
                context.Random);

            double candidateObjective =
                context.Evaluate(
                    candidate,
                    state);

            RequireFiniteObjective(
                candidateObjective);

            worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            bool replacedWorst =
                problem.Sense.IsBetter(
                    candidateObjective,
                    objectiveValues[worstIndex]);

            if (replacedWorst)
            {
                candidate.AsSpan().CopyTo(
                    harmonyMemory[worstIndex]);

                objectiveValues[worstIndex] =
                    candidateObjective;
            }

            bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                new AdvancedParameterSettingFreeHarmonySearchIterationState(
                    Iteration: improvisation,
                    Phase: HarmonySearchPhase.CompletedImprovisation,
                    HarmonyMemorySize: harmonyMemorySize,
                    TotalImprovisations: improvisation,
                    HarmonyMemoryConsiderationRate: hmcr,
                    PitchAdjustmentRate: par,
                    MinimumPitchAdjustmentBandwidth: minimumBandwidth,
                    MaximumPitchAdjustmentBandwidth: maximumBandwidth,
                    ReplacedWorstHarmony: replacedWorst,
                    MemoryBestFitness: objectiveValues[bestIndex],
                    MemoryWorstFitness: objectiveValues[worstIndex]);

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
                "MaximumAdvancedParameterSettingFreeHarmonySearchIterationImprovisations",
                "The configured iteration Advanced PSF-HS improvisation limit was reached."),
            state);
    }

    private static void ImproviseHarmony(
        double[][] harmonyMemory,
        Span<double> destination,
        double hmcr,
        double par,
        double bandwidthFraction,
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
            double lower =
                lowerBounds[coordinate];

            double upper =
                upperBounds[coordinate];

            double value;

            if (random.NextDouble() <
                hmcr)
            {
                int sourceHarmony =
                    random.NextInt32(
                        harmonyMemory.Length);

                value =
                    harmonyMemory[sourceHarmony][coordinate];

                if (random.NextDouble() <
                    par)
                {
                    double bandwidth =
                        bandwidthFraction *
                        (upper - lower);

                    value +=
                        ((2.0 * random.NextDouble()) - 1.0) *
                        bandwidth;
                }
            }
            else
            {
                value =
                    lower +
                    (random.NextDouble() *
                     (upper - lower));
            }

            if (!double.IsFinite(value))
            {
                throw new InvalidOperationException(
                    "Iteration Advanced PSF-HS produced a non-finite coordinate.");
            }

            destination[coordinate] =
                value;
        }

        searchSpace.Clamp(
            destination);
    }

    private static void ValidateFinitePositiveSpans(
        IBoundedContinuousSearchSpace searchSpace)
    {
        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        for (int i = 0;
             i < lower.Length;
             i++)
        {
            double span =
                upper[i] -
                lower[i];

            if (!double.IsFinite(span) ||
                span <= 0.0)
            {
                throw new InvalidOperationException(
                    "Iteration Advanced PSF-HS requires finite positive coordinate spans.");
            }
        }
    }

    private static void GetBandwidthRange(
        IBoundedContinuousSearchSpace searchSpace,
        double bandwidthFraction,
        out double minimum,
        out double maximum)
    {
        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        minimum =
            double.PositiveInfinity;

        maximum =
            double.NegativeInfinity;

        for (int i = 0;
             i < lower.Length;
             i++)
        {
            double bandwidth =
                bandwidthFraction *
                (upper[i] - lower[i]);

            minimum =
                Math.Min(
                    minimum,
                    bandwidth);

            maximum =
                Math.Max(
                    maximum,
                    bandwidth);
        }
    }

    private static int FindBestIndex(
        ReadOnlySpan<double> objectiveValues,
        OptimizationSense sense)
    {
        int bestIndex = 0;

        for (int i = 1;
             i < objectiveValues.Length;
             i++)
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

        for (int i = 1;
             i < objectiveValues.Length;
             i++)
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
                "Iteration Advanced PSF-HS requires finite objective values.");
        }
    }
}
