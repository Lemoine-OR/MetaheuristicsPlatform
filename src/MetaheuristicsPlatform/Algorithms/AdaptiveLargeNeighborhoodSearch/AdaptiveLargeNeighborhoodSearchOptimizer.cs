using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public sealed class AdaptiveLargeNeighborhoodSearchOptimizer<TSolution,TRemoved> :
    IMetaheuristic<TSolution,AdaptiveLargeNeighborhoodSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initial;
    private readonly AdaptiveLargeNeighborhoodDestroyOperator<TSolution,TRemoved>[] _destroyOperators;
    private readonly AdaptiveLargeNeighborhoodRepairOperator<TSolution,TRemoved>[] _repairOperators;
    private readonly IEqualityComparer<TSolution> _solutionComparer;
    private readonly ILargeNeighborhoodAcceptancePolicy? _acceptanceOverride;
    private readonly IAdaptiveLargeNeighborhoodOperatorSelectionStrategy _selectionStrategy;

    public AdaptiveLargeNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IEnumerable<AdaptiveLargeNeighborhoodDestroyOperator<TSolution,TRemoved>> destroyOperators,
        IEnumerable<AdaptiveLargeNeighborhoodRepairOperator<TSolution,TRemoved>> repairOperators,
        IEqualityComparer<TSolution> solutionComparer)
        : this(
            initialSolutionGenerator,
            destroyOperators,
            repairOperators,
            solutionComparer,
            acceptanceOverride: null,
            selectionStrategy: null)
    {
    }

    public AdaptiveLargeNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IEnumerable<AdaptiveLargeNeighborhoodDestroyOperator<TSolution,TRemoved>> destroyOperators,
        IEnumerable<AdaptiveLargeNeighborhoodRepairOperator<TSolution,TRemoved>> repairOperators,
        IEqualityComparer<TSolution> solutionComparer,
        ILargeNeighborhoodAcceptancePolicy? acceptanceOverride)
        : this(
            initialSolutionGenerator,
            destroyOperators,
            repairOperators,
            solutionComparer,
            acceptanceOverride,
            selectionStrategy: null)
    {
    }

    public AdaptiveLargeNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IEnumerable<AdaptiveLargeNeighborhoodDestroyOperator<TSolution,TRemoved>> destroyOperators,
        IEnumerable<AdaptiveLargeNeighborhoodRepairOperator<TSolution,TRemoved>> repairOperators,
        IEqualityComparer<TSolution> solutionComparer,
        ILargeNeighborhoodAcceptancePolicy? acceptanceOverride,
        IAdaptiveLargeNeighborhoodOperatorSelectionStrategy? selectionStrategy)
    {
        _initial =
            initialSolutionGenerator ??
            throw new ArgumentNullException(nameof(initialSolutionGenerator));

        ArgumentNullException.ThrowIfNull(destroyOperators);
        ArgumentNullException.ThrowIfNull(repairOperators);

        _destroyOperators = destroyOperators.ToArray();
        _repairOperators = repairOperators.ToArray();

        _solutionComparer =
            solutionComparer ??
            throw new ArgumentNullException(nameof(solutionComparer));

        _acceptanceOverride =
            acceptanceOverride;

        _selectionStrategy =
            selectionStrategy ??
            IndependentSegmentedRouletteOperatorSelectionStrategy.Instance;

        ValidateOperatorPool(
            _destroyOperators.Select(item => item.Id),
            "destroy");

        ValidateOperatorPool(
            _repairOperators.Select(item => item.Id),
            "repair");
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.AdaptiveLargeNeighborhoodSearch,
            Name = "Adaptive Large Neighborhood Search - Ropke-Pisinger",
            Acronym = "ALNS",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.TrajectoryBased,
            Mechanisms =
                MetaheuristicMechanism.Neighborhood |
                MetaheuristicMechanism.Trajectory |
                MetaheuristicMechanism.Constructive |
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces =
                SearchSpaceKind.Binary |
                SearchSpaceKind.Integer |
                SearchSpaceKind.Permutation |
                SearchSpaceKind.Combinatorial |
                SearchSpaceKind.Mixed,
            IsStochastic = true,
            References =
            [
                AdaptiveLargeNeighborhoodSearchReferences.RopkePisinger2006,
                AdaptiveLargeNeighborhoodSearchReferences.PisingerRopke2007
            ]
        };

    public AdaptiveLargeNeighborhoodSearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        AdaptiveLargeNeighborhoodSearchParameters parameters,
        ISolutionCloner<TSolution> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<TSolution>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);

        parameters.Validate();

        var context =
            new OptimizationContext<TSolution>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        ILargeNeighborhoodAcceptancePolicy acceptance =
            _acceptanceOverride ??
            new GeometricSimulatedAnnealingLargeNeighborhoodAcceptancePolicy(
                parameters.InitialTemperature,
                parameters.CoolingRate);

        IAdaptiveLargeNeighborhoodOperatorSelectionSession selectionSession =
            _selectionStrategy.CreateSession(
                _destroyOperators.Length,
                _repairOperators.Length,
                parameters);

        var visited =
            new HashSet<TSolution>(
                _solutionComparer);

        long destroyInvocations = 0;
        long repairInvocations = 0;
        long acceptedCandidates = 0;
        long rejectedCandidates = 0;
        long segmentWeightUpdates = 0;

        var initialState =
            CreateState(
                iterationsCompleted: 0,
                segment: 1,
                iterationInSegment: 0,
                currentObjective: double.NaN,
                bestObjective: problem.Sense.WorstValue(),
                lastCandidateObjective: double.NaN,
                destroyOperatorId: null,
                repairOperatorId: null,
                destroyOperatorWeight: double.NaN,
                repairOperatorWeight: double.NaN,
                lastReward: 0.0,
                lastCandidateAccepted: false,
                lastCandidateNovel: true,
                destroyInvocations,
                repairInvocations,
                acceptedCandidates,
                rejectedCandidates,
                segmentWeightUpdates);

        context.Start(initialState);

        TSolution current =
            _initial.Create(
                problem,
                context.Random);

        double currentObjective =
            context.Evaluate(
                current,
                initialState);

        RequireFiniteObjective(currentObjective);

        visited.Add(
            solutionCloner.Clone(
                current));

        AdaptiveLargeNeighborhoodSearchState state =
            CreateState(
                iterationsCompleted: 0,
                segment: 1,
                iterationInSegment: 0,
                currentObjective,
                bestObjective: context.State.BestFitness,
                lastCandidateObjective: currentObjective,
                destroyOperatorId: null,
                repairOperatorId: null,
                destroyOperatorWeight: double.NaN,
                repairOperatorWeight: double.NaN,
                lastReward: 0.0,
                lastCandidateAccepted: true,
                lastCandidateNovel: true,
                destroyInvocations,
                repairInvocations,
                acceptedCandidates,
                rejectedCandidates,
                segmentWeightUpdates);

        StoppingDecision stop =
            context.EvaluateStopping(
                state);

        if (stop.ShouldStop)
        {
            return context.Complete(
                stop,
                state);
        }

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AdaptiveLargeNeighborhoodOperatorSelection selection =
                selectionSession.Select(
                    context.Random,
                    iteration);

            int destroyIndex =
                selection.DestroyIndex;

            int repairIndex =
                selection.RepairIndex;

            AdaptiveLargeNeighborhoodDestroyOperator<TSolution,TRemoved> destroy =
                _destroyOperators[destroyIndex];

            AdaptiveLargeNeighborhoodRepairOperator<TSolution,TRemoved> repair =
                _repairOperators[repairIndex];

            double bestBefore =
                context.State.BestFitness;

            TSolution candidate =
                solutionCloner.Clone(
                    current);

            TRemoved removed =
                destroy.Operator.Destroy(
                    ref candidate,
                    parameters.DestructionSize,
                    problem,
                    context.Random);

            destroyInvocations++;

            repair.Operator.Repair(
                ref candidate,
                in removed,
                problem,
                context.Random);

            repairInvocations++;

            int segment =
                ((iteration - 1) /
                 parameters.SegmentLength) +
                1;

            int iterationInSegment =
                ((iteration - 1) %
                 parameters.SegmentLength) +
                1;

            state =
                CreateState(
                    iterationsCompleted: iteration - 1,
                    segment,
                    iterationInSegment,
                    currentObjective,
                    bestObjective: bestBefore,
                    lastCandidateObjective: double.NaN,
                    destroyOperatorId: destroy.Id,
                    repairOperatorId: repair.Id,
                    destroyOperatorWeight: selection.DestroySelectionMetric,
                    repairOperatorWeight: selection.RepairSelectionMetric,
                    lastReward: 0.0,
                    lastCandidateAccepted: false,
                    lastCandidateNovel: false,
                    destroyInvocations,
                    repairInvocations,
                    acceptedCandidates,
                    rejectedCandidates,
                    segmentWeightUpdates);

            double candidateObjective =
                context.Evaluate(
                    candidate,
                    state);

            RequireFiniteObjective(candidateObjective);

            bool isNovel =
                visited.Add(
                    solutionCloner.Clone(
                        candidate));

            bool isNewGlobalBest =
                problem.Sense.IsBetter(
                    candidateObjective,
                    bestBefore);

            bool improvesCurrent =
                problem.Sense.IsBetter(
                    candidateObjective,
                    currentObjective);

            state =
                state with
                {
                    BestObjective = context.State.BestFitness,
                    LastCandidateObjective = candidateObjective,
                    LastCandidateNovel = isNovel
                };

            stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    state);
            }

            var acceptanceContext =
                new LargeNeighborhoodAcceptanceContext(
                    problem.Sense,
                    iteration,
                    currentObjective,
                    candidateObjective,
                    bestBefore);

            bool accepted =
                acceptance.ShouldAccept(
                    in acceptanceContext,
                    context.Random);

            double reward =
                AdaptiveLargeNeighborhoodAdaptation.DetermineReward(
                    isNovel,
                    isNewGlobalBest,
                    improvesCurrent,
                    accepted,
                    parameters);

            selectionSession.RecordOutcome(
                in selection,
                reward);

            if (accepted)
            {
                current = candidate;
                currentObjective = candidateObjective;
                acceptedCandidates++;
            }
            else
            {
                rejectedCandidates++;
            }

            selectionSession.CompleteIteration(
                iteration);

            segmentWeightUpdates =
                selectionSession.SegmentUpdateCount;

            AdaptiveLargeNeighborhoodOperatorSelection currentSelectionMetrics =
                selectionSession.GetCurrentSelectionMetrics(
                    in selection,
                    iteration);

            state =
                CreateState(
                    iterationsCompleted: iteration,
                    segment,
                    iterationInSegment,
                    currentObjective,
                    bestObjective: context.State.BestFitness,
                    lastCandidateObjective: candidateObjective,
                    destroyOperatorId: destroy.Id,
                    repairOperatorId: repair.Id,
                    destroyOperatorWeight: currentSelectionMetrics.DestroySelectionMetric,
                    repairOperatorWeight: currentSelectionMetrics.RepairSelectionMetric,
                    lastReward: reward,
                    lastCandidateAccepted: accepted,
                    lastCandidateNovel: isNovel,
                    destroyInvocations,
                    repairInvocations,
                    acceptedCandidates,
                    rejectedCandidates,
                    segmentWeightUpdates);

            context.CompleteIteration(
                currentObjective,
                state);

            stop =
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
                "MaximumAdaptiveLargeNeighborhoodSearchIterations",
                "The configured Adaptive Large Neighborhood Search iteration limit was reached."),
            state);
    }

    private static void ValidateOperatorPool(
        IEnumerable<string> ids,
        string poolName)
    {
        string[] materialized =
            ids.ToArray();

        if (materialized.Length == 0)
        {
            throw new ArgumentException(
                $"Adaptive LNS {poolName} operator pool must be non-empty.");
        }

        var seen =
            new HashSet<string>(
                StringComparer.Ordinal);

        foreach (string id in materialized)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    $"Adaptive LNS {poolName} operator IDs must be non-empty.");
            }

            if (!seen.Add(id))
            {
                throw new ArgumentException(
                    $"Adaptive LNS {poolName} operator ID '{id}' is duplicated.");
            }
        }
    }

    private static AdaptiveLargeNeighborhoodSearchState CreateState(
        int iterationsCompleted,
        int segment,
        int iterationInSegment,
        double currentObjective,
        double bestObjective,
        double lastCandidateObjective,
        string? destroyOperatorId,
        string? repairOperatorId,
        double destroyOperatorWeight,
        double repairOperatorWeight,
        double lastReward,
        bool lastCandidateAccepted,
        bool lastCandidateNovel,
        long destroyInvocations,
        long repairInvocations,
        long acceptedCandidates,
        long rejectedCandidates,
        long segmentWeightUpdates) =>
        new(
            iterationsCompleted,
            segment,
            iterationInSegment,
            currentObjective,
            bestObjective,
            lastCandidateObjective,
            destroyOperatorId,
            repairOperatorId,
            destroyOperatorWeight,
            repairOperatorWeight,
            lastReward,
            lastCandidateAccepted,
            lastCandidateNovel,
            destroyInvocations,
            repairInvocations,
            acceptedCandidates,
            rejectedCandidates,
            segmentWeightUpdates);

    private static void RequireFiniteObjective(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Adaptive Large Neighborhood Search requires finite objective values.");
        }
    }
}
