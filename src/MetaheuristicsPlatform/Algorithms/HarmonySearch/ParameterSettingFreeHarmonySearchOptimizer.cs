using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameter-Setting-Free Harmony Search following Geem and Sim (2010).
/// </summary>
/// <remarks>
/// PSF-HS augments Harmony Memory with an Operation Type Matrix. During rehearsal,
/// HMCR and PAR are fixed at 0.5. During performance, each decision variable gets
/// its own HMCR/PAR from the operation types currently stored in the OTM.
/// </remarks>
public sealed class ParameterSettingFreeHarmonySearchOptimizer :
    IMetaheuristic<double[], ParameterSettingFreeHarmonySearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ParameterSettingFreeHarmonySearch,
            Name = "Parameter-Setting-Free Harmony Search",
            Acronym = "PSF-HS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms =
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                HarmonySearchReferences.GeemSim2010ParameterSettingFree
            ]
        };

    public ParameterSettingFreeHarmonySearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ParameterSettingFreeHarmonySearchParameters parameters,
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
                "Parameter-Setting-Free Harmony Search requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Parameter-Setting-Free Harmony Search requires a positive search-space dimension.");
        }

        int harmonyMemorySize =
            parameters.HarmonyMemorySize;

        int rehearsalImprovisations =
            parameters.GetRehearsalImprovisations();

        double[][] harmonyMemory =
            new double[harmonyMemorySize][];

        double[] objectiveValues =
            new double[harmonyMemorySize];

        var operationTypeMemory =
            new ParameterSettingFreeHarmonySearchOperationType[
                harmonyMemorySize,
                dimension];

        for (int row = 0;
             row < harmonyMemorySize;
             row++)
        {
            harmonyMemory[row] =
                new double[dimension];

            for (int coordinate = 0;
                 coordinate < dimension;
                 coordinate++)
            {
                operationTypeMemory[row, coordinate] =
                    ParameterSettingFreeHarmonySearchOperationType.RandomSelection;
            }
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

        ParameterSettingFreeHarmonySearchState state =
            BuildState(
                iteration: 0,
                phase: HarmonySearchPhase.Initialization,
                stage: ParameterSettingFreeHarmonySearchStage.RandomTuning,
                harmonyMemorySize,
                totalImprovisations: 0,
                rehearsalImprovisations,
                operationTypeMemory,
                fixedHmcr:
                    ParameterSettingFreeHarmonySearchParameters
                        .RehearsalHarmonyMemoryConsiderationRate,
                fixedPar:
                    ParameterSettingFreeHarmonySearchParameters
                        .RehearsalPitchAdjustmentRate,
                replacedWorstHarmony: false,
                memoryBestFitness: null,
                memoryWorstFitness: null);

        context.Start(state);

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

        var candidateOperations =
            new ParameterSettingFreeHarmonySearchOperationType[
                dimension];

        double[] hmcr =
            new double[dimension];

        double[] par =
            new double[dimension];

        for (int improvisation = 1;
             improvisation <= parameters.MaximumImprovisations;
             improvisation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ParameterSettingFreeHarmonySearchStage stage =
                improvisation <= rehearsalImprovisations
                    ? ParameterSettingFreeHarmonySearchStage.Rehearsal
                    : ParameterSettingFreeHarmonySearchStage.Performance;

            if (stage ==
                ParameterSettingFreeHarmonySearchStage.Rehearsal)
            {
                Array.Fill(
                    hmcr,
                    ParameterSettingFreeHarmonySearchParameters
                        .RehearsalHarmonyMemoryConsiderationRate);

                Array.Fill(
                    par,
                    ParameterSettingFreeHarmonySearchParameters
                        .RehearsalPitchAdjustmentRate);
            }
            else
            {
                CalculateAdaptiveRates(
                    operationTypeMemory,
                    hmcr,
                    par);
            }

            int worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            state =
                BuildState(
                    iteration: improvisation - 1,
                    phase: HarmonySearchPhase.Improvisation,
                    stage,
                    harmonyMemorySize,
                    totalImprovisations: improvisation - 1,
                    rehearsalImprovisations,
                    operationTypeMemory,
                    hmcr,
                    par,
                    replacedWorstHarmony: false,
                    memoryBestFitness:
                        objectiveValues[
                            FindBestIndex(
                                objectiveValues,
                                problem.Sense)],
                    memoryWorstFitness:
                        objectiveValues[worstIndex]);

            ImproviseHarmony(
                harmonyMemory,
                candidate,
                candidateOperations,
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

                for (int coordinate = 0;
                     coordinate < dimension;
                     coordinate++)
                {
                    operationTypeMemory[worstIndex, coordinate] =
                        candidateOperations[coordinate];
                }
            }

            int bestIndex =
                FindBestIndex(
                    objectiveValues,
                    problem.Sense);

            worstIndex =
                FindWorstIndex(
                    objectiveValues,
                    problem.Sense);

            if (stage ==
                ParameterSettingFreeHarmonySearchStage.Performance)
            {
                CalculateAdaptiveRates(
                    operationTypeMemory,
                    hmcr,
                    par);
            }

            state =
                BuildState(
                    iteration: improvisation,
                    phase: HarmonySearchPhase.CompletedImprovisation,
                    stage,
                    harmonyMemorySize,
                    totalImprovisations: improvisation,
                    rehearsalImprovisations,
                    operationTypeMemory,
                    hmcr,
                    par,
                    replacedWorstHarmony: replacedWorst,
                    memoryBestFitness:
                        objectiveValues[bestIndex],
                    memoryWorstFitness:
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
                "MaximumParameterSettingFreeHarmonySearchImprovisations",
                "The configured Parameter-Setting-Free Harmony Search improvisation limit was reached."),
            state);
    }

    private static void CalculateAdaptiveRates(
        ParameterSettingFreeHarmonySearchOperationType[,] operationTypeMemory,
        Span<double> hmcr,
        Span<double> par)
    {
        int rows =
            operationTypeMemory.GetLength(0);

        int dimension =
            operationTypeMemory.GetLength(1);

        for (int coordinate = 0;
             coordinate < dimension;
             coordinate++)
        {
            int memoryCount = 0;
            int pitchCount = 0;

            for (int row = 0;
                 row < rows;
                 row++)
            {
                switch (operationTypeMemory[row, coordinate])
                {
                    case ParameterSettingFreeHarmonySearchOperationType.MemoryConsideration:
                        memoryCount++;
                        break;

                    case ParameterSettingFreeHarmonySearchOperationType.PitchAdjustment:
                        pitchCount++;
                        break;
                }
            }

            int memoryOrPitch =
                memoryCount +
                pitchCount;

            hmcr[coordinate] =
                (double)memoryOrPitch /
                rows;

            // The paper's PAR denominator is count(Memory or Pitch).
            // If that count is zero, HMCR is exactly zero and the pitch branch
            // is unreachable. PAR=0 is the platform's explicit defensive
            // completion of this otherwise undefined 0/0 corner case.
            par[coordinate] =
                memoryOrPitch == 0
                    ? 0.0
                    : (double)pitchCount /
                      memoryOrPitch;
        }
    }

    private static void ImproviseHarmony(
        double[][] harmonyMemory,
        Span<double> destination,
        Span<ParameterSettingFreeHarmonySearchOperationType> operations,
        ReadOnlySpan<double> hmcr,
        ReadOnlySpan<double> par,
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
                hmcr[coordinate])
            {
                int sourceHarmony =
                    random.NextInt32(
                        harmonyMemory.Length);

                value =
                    harmonyMemory[sourceHarmony][coordinate];

                if (random.NextDouble() <
                    par[coordinate])
                {
                    double bandwidth =
                        bandwidthFraction *
                        (upper - lower);

                    value +=
                        ((2.0 * random.NextDouble()) - 1.0) *
                        bandwidth;

                    operations[coordinate] =
                        ParameterSettingFreeHarmonySearchOperationType.PitchAdjustment;
                }
                else
                {
                    operations[coordinate] =
                        ParameterSettingFreeHarmonySearchOperationType.MemoryConsideration;
                }
            }
            else
            {
                value =
                    lower +
                    (random.NextDouble() *
                     (upper - lower));

                operations[coordinate] =
                    ParameterSettingFreeHarmonySearchOperationType.RandomSelection;
            }

            if (!double.IsFinite(value))
            {
                throw new InvalidOperationException(
                    "Parameter-Setting-Free Harmony Search produced a non-finite coordinate.");
            }

            destination[coordinate] =
                value;
        }

        searchSpace.Clamp(
            destination);
    }

    private static ParameterSettingFreeHarmonySearchState BuildState(
        int iteration,
        HarmonySearchPhase phase,
        ParameterSettingFreeHarmonySearchStage stage,
        int harmonyMemorySize,
        int totalImprovisations,
        int rehearsalImprovisations,
        ParameterSettingFreeHarmonySearchOperationType[,] operationTypeMemory,
        double fixedHmcr,
        double fixedPar,
        bool replacedWorstHarmony,
        double? memoryBestFitness,
        double? memoryWorstFitness)
    {
        return BuildState(
            iteration,
            phase,
            stage,
            harmonyMemorySize,
            totalImprovisations,
            rehearsalImprovisations,
            operationTypeMemory,
            ReadOnlySpan<double>.Empty,
            ReadOnlySpan<double>.Empty,
            replacedWorstHarmony,
            memoryBestFitness,
            memoryWorstFitness,
            fixedHmcr,
            fixedPar);
    }

    private static ParameterSettingFreeHarmonySearchState BuildState(
        int iteration,
        HarmonySearchPhase phase,
        ParameterSettingFreeHarmonySearchStage stage,
        int harmonyMemorySize,
        int totalImprovisations,
        int rehearsalImprovisations,
        ParameterSettingFreeHarmonySearchOperationType[,] operationTypeMemory,
        ReadOnlySpan<double> hmcr,
        ReadOnlySpan<double> par,
        bool replacedWorstHarmony,
        double? memoryBestFitness,
        double? memoryWorstFitness,
        double? fixedHmcr = null,
        double? fixedPar = null)
    {
        int randomCount = 0;
        int memoryCount = 0;
        int pitchCount = 0;

        foreach (ParameterSettingFreeHarmonySearchOperationType operation
                 in operationTypeMemory)
        {
            switch (operation)
            {
                case ParameterSettingFreeHarmonySearchOperationType.RandomSelection:
                    randomCount++;
                    break;

                case ParameterSettingFreeHarmonySearchOperationType.MemoryConsideration:
                    memoryCount++;
                    break;

                case ParameterSettingFreeHarmonySearchOperationType.PitchAdjustment:
                    pitchCount++;
                    break;
            }
        }

        double minimumHmcr =
            fixedHmcr ??
            (hmcr.Length == 0
                ? 0.0
                : Minimum(hmcr));

        double maximumHmcr =
            fixedHmcr ??
            (hmcr.Length == 0
                ? 0.0
                : Maximum(hmcr));

        double minimumPar =
            fixedPar ??
            (par.Length == 0
                ? 0.0
                : Minimum(par));

        double maximumPar =
            fixedPar ??
            (par.Length == 0
                ? 0.0
                : Maximum(par));

        return new ParameterSettingFreeHarmonySearchState(
            Iteration: iteration,
            Phase: phase,
            Stage: stage,
            HarmonyMemorySize: harmonyMemorySize,
            TotalImprovisations: totalImprovisations,
            RehearsalImprovisations: rehearsalImprovisations,
            RandomOperationCount: randomCount,
            MemoryOperationCount: memoryCount,
            PitchOperationCount: pitchCount,
            MinimumHarmonyMemoryConsiderationRate: minimumHmcr,
            MaximumHarmonyMemoryConsiderationRate: maximumHmcr,
            MinimumPitchAdjustmentRate: minimumPar,
            MaximumPitchAdjustmentRate: maximumPar,
            ReplacedWorstHarmony: replacedWorstHarmony,
            MemoryBestFitness: memoryBestFitness,
            MemoryWorstFitness: memoryWorstFitness);
    }

    private static double Minimum(
        ReadOnlySpan<double> values)
    {
        double result =
            values[0];

        for (int i = 1;
             i < values.Length;
             i++)
        {
            result =
                Math.Min(
                    result,
                    values[i]);
        }

        return result;
    }

    private static double Maximum(
        ReadOnlySpan<double> values)
    {
        double result =
            values[0];

        for (int i = 1;
             i < values.Length;
             i++)
        {
            result =
                Math.Max(
                    result,
                    values[i]);
        }

        return result;
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
                "Parameter-Setting-Free Harmony Search requires finite objective values.");
        }
    }
}
