using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Object-dependent Advanced Parameter-Setting-Free Harmony Search
/// following Jeong, Park, Geem and Sim (2020).
/// </summary>
/// <remarks>
/// This identity implements Equations (7), (8) and the adaptive bandwidth
/// Equation (9). It is intentionally minimization-only because Equation (7)
/// is published for finding the global minimum.
/// </remarks>
public sealed class AdvancedParameterSettingFreeHarmonySearchObjectOptimizer :
    IMetaheuristic<
        double[],
        AdvancedParameterSettingFreeHarmonySearchObjectParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id =
                MetaheuristicAlgorithmIds
                    .AdvancedParameterSettingFreeHarmonySearchObject,
            Name =
                "Advanced Parameter-Setting-Free Harmony Search - Object Scheme",
            Acronym = "APSF-HS-O",
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

    public AdvancedParameterSettingFreeHarmonySearchObjectParameters
        CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        AdvancedParameterSettingFreeHarmonySearchObjectParameters parameters,
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

        if (problem.Sense !=
            OptimizationSense.Minimize)
        {
            throw new NotSupportedException(
                "The published Object PSF-HS Equation (7) is minimization-only.");
        }

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "Object Advanced PSF-HS requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Object Advanced PSF-HS requires positive dimension.");
        }

        ValidateFinitePositiveSpans(
            searchSpace);

        int hms =
            parameters.HarmonyMemorySize;

        double[][] harmonyMemory =
            new double[hms][];

        double[] objectiveValues =
            new double[hms];

        for (int row = 0;
             row < hms;
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

        double bandwidthFraction =
            parameters.InitialPitchAdjustmentBandwidthFractionOfRange;

        AdvancedParameterSettingFreeHarmonySearchObjectState state =
            BuildState(
                iteration: 0,
                phase: HarmonySearchPhase.Initialization,
                isRehearsal: true,
                hms,
                totalImprovisations: 0,
                completedBandwidthBlocks: 0,
                parameters.TargetObjective,
                hmcr: parameters.RehearsalHarmonyMemoryConsiderationRate,
                par: parameters.RehearsalPitchAdjustmentRate,
                mean: double.NaN,
                lossStart: null,
                bandwidthFraction,
                replacedWorst: false,
                best: null,
                worst: null);

        context.Start(
            state);

        for (int row = 0;
             row < hms;
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

            int bestIndex =
                FindBestIndex(
                    objectiveValues.AsSpan(0, row + 1));

            if (objectiveValues[bestIndex] <=
                parameters.TargetObjective)
            {
                return context.Complete(
                    StoppingDecision.Stop(
                        "AdvancedParameterSettingFreeHarmonySearchObjectTarget",
                        "Object PSF-HS reached its target objective during initialization."),
                    state);
            }

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

        // Paper: perform HMS improvisations with fixed HMCR/PAR before Loss_start.
        int totalImprovisations = 0;

        for (int rehearsal = 1;
             rehearsal <= hms &&
             totalImprovisations < parameters.MaximumImprovisations;
             rehearsal++)
        {
            totalImprovisations++;

            state =
                RunOneImprovisation(
                    context,
                    searchSpace,
                    harmonyMemory,
                    objectiveValues,
                    candidate,
                    parameters.RehearsalHarmonyMemoryConsiderationRate,
                    parameters.RehearsalPitchAdjustmentRate,
                    bandwidthFraction,
                    parameters,
                    totalImprovisations,
                    isRehearsal: true,
                    completedBandwidthBlocks: 0,
                    lossStart: null);

            if (state.MemoryBestFitness <=
                parameters.TargetObjective)
            {
                return context.Complete(
                    StoppingDecision.Stop(
                        "AdvancedParameterSettingFreeHarmonySearchObjectTarget",
                        "Object PSF-HS reached its target objective during rehearsal."),
                    state);
            }

            context.CompleteIteration(
                state.MemoryBestFitness!.Value,
                state);

            StoppingDecision rehearsalStop =
                context.EvaluateStopping(
                    state);

            if (rehearsalStop.ShouldStop)
            {
                return context.Complete(
                    rehearsalStop,
                    state);
            }
        }

        if (totalImprovisations >=
            parameters.MaximumImprovisations)
        {
            return context.Complete(
                StoppingDecision.Stop(
                    "MaximumAdvancedParameterSettingFreeHarmonySearchObjectImprovisations",
                    "The configured Object Advanced PSF-HS safety limit was reached during rehearsal."),
                state);
        }

        double lossStart =
            Mean(
                objectiveValues);

        if (!(lossStart >
              parameters.TargetObjective))
        {
            return context.Complete(
                StoppingDecision.Stop(
                    "AdvancedParameterSettingFreeHarmonySearchObjectTarget",
                    "Object PSF-HS reached its target objective at Loss_start."),
                state);
        }

        double previousBlockMean =
            lossStart;

        int completedBandwidthBlocks = 0;
        int positionInBlock = 0;

        while (totalImprovisations <
               parameters.MaximumImprovisations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double currentMean =
                Mean(
                    objectiveValues);

            double hmcr =
                parameters.GetObjectHarmonyMemoryConsiderationRate(
                    currentMean,
                    lossStart,
                    dimension);

            double par =
                AdvancedParameterSettingFreeHarmonySearchObjectParameters
                    .GetPitchAdjustmentRate(
                        hmcr,
                        dimension);

            totalImprovisations++;
            positionInBlock++;

            state =
                RunOneImprovisation(
                    context,
                    searchSpace,
                    harmonyMemory,
                    objectiveValues,
                    candidate,
                    hmcr,
                    par,
                    bandwidthFraction,
                    parameters,
                    totalImprovisations,
                    isRehearsal: false,
                    completedBandwidthBlocks,
                    lossStart);

            if (state.MemoryBestFitness <=
                parameters.TargetObjective)
            {
                context.CompleteIteration(
                    state.MemoryBestFitness.Value,
                    state);

                return context.Complete(
                    StoppingDecision.Stop(
                        "AdvancedParameterSettingFreeHarmonySearchObjectTarget",
                        "Object PSF-HS reached its target objective."),
                    state);
            }

            if (positionInBlock == hms)
            {
                double blockMean =
                    Mean(
                        objectiveValues);

                bandwidthFraction =
                    parameters.GetAdaptiveBandwidthFraction(
                        previousBlockMean,
                        blockMean,
                        lossStart);

                if (!double.IsFinite(bandwidthFraction) ||
                    bandwidthFraction < 0.0)
                {
                    throw new InvalidOperationException(
                        "Object PSF-HS Equation (9) produced an invalid bandwidth fraction.");
                }

                previousBlockMean =
                    blockMean;

                positionInBlock = 0;
                completedBandwidthBlocks++;
            }

            state =
                state with
                {
                    CompletedAdaptiveBandwidthBlocks =
                        completedBandwidthBlocks,
                    CurrentBandwidthFractionOfRange =
                        bandwidthFraction,
                    CurrentHarmonyMemoryMean =
                        Mean(objectiveValues)
                };

            context.CompleteIteration(
                state.MemoryBestFitness!.Value,
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
                "MaximumAdvancedParameterSettingFreeHarmonySearchObjectImprovisations",
                "The configured Object Advanced PSF-HS safety limit was reached."),
            state);
    }

    private static AdvancedParameterSettingFreeHarmonySearchObjectState
        RunOneImprovisation(
            OptimizationContext<double[]> context,
            IBoundedContinuousSearchSpace searchSpace,
            double[][] harmonyMemory,
            double[] objectiveValues,
            double[] candidate,
            double hmcr,
            double par,
            double bandwidthFraction,
            AdvancedParameterSettingFreeHarmonySearchObjectParameters parameters,
            int totalImprovisations,
            bool isRehearsal,
            int completedBandwidthBlocks,
            double? lossStart)
    {
        int bestIndex =
            FindBestIndex(
                objectiveValues);

        int worstIndex =
            FindWorstIndex(
                objectiveValues);

        double currentMean =
            Mean(
                objectiveValues);

        AdvancedParameterSettingFreeHarmonySearchObjectState state =
            BuildState(
                iteration: totalImprovisations - 1,
                phase: HarmonySearchPhase.Improvisation,
                isRehearsal,
                harmonyMemory.Length,
                totalImprovisations: totalImprovisations - 1,
                completedBandwidthBlocks,
                parameters.TargetObjective,
                hmcr,
                par,
                currentMean,
                lossStart,
                bandwidthFraction,
                replacedWorst: false,
                best: objectiveValues[bestIndex],
                worst: objectiveValues[worstIndex]);

        ImproviseHarmony(
            harmonyMemory,
            candidate,
            hmcr,
            par,
            bandwidthFraction,
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
                objectiveValues);

        bool replacedWorst =
            candidateObjective <
            objectiveValues[worstIndex];

        if (replacedWorst)
        {
            candidate.AsSpan().CopyTo(
                harmonyMemory[worstIndex]);

            objectiveValues[worstIndex] =
                candidateObjective;
        }

        bestIndex =
            FindBestIndex(
                objectiveValues);

        worstIndex =
            FindWorstIndex(
                objectiveValues);

        return
            BuildState(
                iteration: totalImprovisations,
                phase: HarmonySearchPhase.CompletedImprovisation,
                isRehearsal,
                harmonyMemory.Length,
                totalImprovisations,
                completedBandwidthBlocks,
                parameters.TargetObjective,
                hmcr,
                par,
                Mean(objectiveValues),
                lossStart,
                bandwidthFraction,
                replacedWorst,
                objectiveValues[bestIndex],
                objectiveValues[worstIndex]);
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
        ReadOnlySpan<double> lower =
            searchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            searchSpace.UpperBounds;

        for (int coordinate = 0;
             coordinate < destination.Length;
             coordinate++)
        {
            double value;

            if (random.NextDouble() <
                hmcr)
            {
                int source =
                    random.NextInt32(
                        harmonyMemory.Length);

                value =
                    harmonyMemory[source][coordinate];

                if (random.NextDouble() <
                    par)
                {
                    double bandwidth =
                        bandwidthFraction *
                        (upper[coordinate] -
                         lower[coordinate]);

                    value +=
                        ((2.0 * random.NextDouble()) - 1.0) *
                        bandwidth;
                }
            }
            else
            {
                value =
                    lower[coordinate] +
                    random.NextDouble() *
                    (upper[coordinate] -
                     lower[coordinate]);
            }

            destination[coordinate] =
                value;
        }

        searchSpace.Clamp(
            destination);
    }

    private static AdvancedParameterSettingFreeHarmonySearchObjectState
        BuildState(
            int iteration,
            HarmonySearchPhase phase,
            bool isRehearsal,
            int hms,
            int totalImprovisations,
            int completedBandwidthBlocks,
            double target,
            double hmcr,
            double par,
            double mean,
            double? lossStart,
            double bandwidthFraction,
            bool replacedWorst,
            double? best,
            double? worst) =>
        new(
            Iteration: iteration,
            Phase: phase,
            IsRehearsal: isRehearsal,
            HarmonyMemorySize: hms,
            TotalImprovisations: totalImprovisations,
            CompletedAdaptiveBandwidthBlocks: completedBandwidthBlocks,
            TargetObjective: target,
            HarmonyMemoryConsiderationRate: hmcr,
            PitchAdjustmentRate: par,
            CurrentHarmonyMemoryMean: mean,
            LossStart: lossStart,
            CurrentBandwidthFractionOfRange: bandwidthFraction,
            ReplacedWorstHarmony: replacedWorst,
            MemoryBestFitness: best,
            MemoryWorstFitness: worst);

    private static double Mean(
        ReadOnlySpan<double> values)
    {
        double sum = 0.0;

        for (int i = 0;
             i < values.Length;
             i++)
        {
            sum +=
                values[i];
        }

        return
            sum /
            values.Length;
    }

    private static int FindBestIndex(
        ReadOnlySpan<double> values)
    {
        int index = 0;

        for (int i = 1;
             i < values.Length;
             i++)
        {
            if (values[i] <
                values[index])
            {
                index = i;
            }
        }

        return index;
    }

    private static int FindWorstIndex(
        ReadOnlySpan<double> values)
    {
        int index = 0;

        for (int i = 1;
             i < values.Length;
             i++)
        {
            if (values[i] >
                values[index])
            {
                index = i;
            }
        }

        return index;
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
                    "Object Advanced PSF-HS requires finite positive coordinate spans.");
            }
        }
    }

    private static void RequireFiniteObjective(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Object Advanced PSF-HS requires finite objective values.");
        }
    }
}
