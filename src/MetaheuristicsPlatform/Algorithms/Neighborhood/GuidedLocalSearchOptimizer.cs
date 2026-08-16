using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Canonical Guided Local Search (GLS) of Voudouris and Tsang.
/// Local descent is performed on an augmented objective while the common
/// <see cref="OptimizationContext{TSolution}"/> continues to own the original
/// objective, best-so-far state, accounting, callbacks and stopping.
/// </summary>
public sealed class GuidedLocalSearchOptimizer<
    TSolution,
    TMove,
    TUndo,
    TMoveEnumerator,
    TFeature,
    TFeatureEnumerator> :
    IMetaheuristic<TSolution, GuidedLocalSearchParameters>
    where TMoveEnumerator : struct, INeighborhoodEnumerator<TMove>
    where TFeature : notnull
    where TFeatureEnumerator : struct, IGuidedLocalSearchFeatureEnumerator<TFeature>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly IEnumeratedNeighborhood<TSolution, TMove, TMoveEnumerator> _neighborhood;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo> _moveOperator;
    private readonly IGuidedLocalSearchFeatureModel<
        TSolution,
        TFeature,
        TFeatureEnumerator> _featureModel;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>? _objectiveDeltaEvaluator;
    private readonly IGuidedLocalSearchPenaltyDeltaEvaluator<
        TSolution,
        TMove,
        TFeature>? _penaltyDeltaEvaluator;
    private readonly IMoveApplicability<TSolution, TMove>? _moveApplicability;
    private readonly IEqualityComparer<TFeature>? _featureComparer;

    /// <summary>Creates a canonical generic Guided Local Search optimizer.</summary>
    public GuidedLocalSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IEnumeratedNeighborhood<TSolution, TMove, TMoveEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        IGuidedLocalSearchFeatureModel<
            TSolution,
            TFeature,
            TFeatureEnumerator> featureModel,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? objectiveDeltaEvaluator = null,
        IGuidedLocalSearchPenaltyDeltaEvaluator<
            TSolution,
            TMove,
            TFeature>? penaltyDeltaEvaluator = null,
        IMoveApplicability<TSolution, TMove>? moveApplicability = null,
        IEqualityComparer<TFeature>? featureComparer = null)
    {
        _initialGenerator =
            initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _neighborhood =
            neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        _moveOperator =
            moveOperator ?? throw new ArgumentNullException(nameof(moveOperator));
        _featureModel =
            featureModel ?? throw new ArgumentNullException(nameof(featureModel));
        _objectiveDeltaEvaluator = objectiveDeltaEvaluator;
        _penaltyDeltaEvaluator = penaltyDeltaEvaluator;
        _moveApplicability = moveApplicability;
        _featureComparer = featureComparer;
    }

    /// <inheritdoc />
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "guided-local-search-voudouris-tsang-1999",
        Name = "Guided Local Search - Voudouris-Tsang",
        Acronym = "GLS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood |
                     MetaheuristicMechanism.Trajectory |
                     MetaheuristicMechanism.MemoryBased,
        SearchSpaces = SearchSpaceKind.Continuous |
                       SearchSpaceKind.Binary |
                       SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation |
                       SearchSpaceKind.Combinatorial |
                       SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            GuidedLocalSearchReferences.TsangVoudouris1997,
            GuidedLocalSearchReferences.VoudourisTsang1999
        }
    };

    /// <inheritdoc />
    public GuidedLocalSearchParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        GuidedLocalSearchParameters parameters,
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

        var context = new OptimizationContext<TSolution>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        context.Start();

        TSolution current = _initialGenerator.Create(problem, context.Random);
        double currentObjective = context.Evaluate(current);

        var penalties = _featureComparer is null
            ? new Dictionary<TFeature, int>()
            : new Dictionary<TFeature, int>(_featureComparer);

        var maximumUtilityFeatures = new List<TFeature>();

        long currentPenaltySum = 0;
        long acceptedMoves = 0;
        long totalPenaltyIncrements = 0;

        StoppingDecision stop = context.EvaluateStopping(
            CreateState(
                0,
                acceptedMoves,
                penalties,
                totalPenaltyIncrements,
                currentObjective,
                currentPenaltySum,
                problem.Sense,
                parameters.PenaltyWeight));

        if (stop.ShouldStop)
        {
            return context.Complete(stop);
        }

        GuidedDescentResult initialDescent = Descend(
            ref current,
            currentObjective,
            currentPenaltySum,
            penaltyUpdates: 0,
            acceptedMoves,
            totalPenaltyIncrements,
            penalties,
            parameters,
            context,
            solutionCloner,
            cancellationToken);

        currentObjective = initialDescent.Objective;
        currentPenaltySum = initialDescent.PenaltySum;
        acceptedMoves = initialDescent.AcceptedMoves;

        if (initialDescent.StoppingDecision.ShouldStop)
        {
            return context.Complete(initialDescent.StoppingDecision);
        }

        for (int penaltyUpdate = 1;
             penaltyUpdate <= parameters.MaximumPenaltyUpdates;
             penaltyUpdate++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            maximumUtilityFeatures.Clear();
            double maximumUtility = FindMaximumUtilityFeatures(
                in current,
                penalties,
                maximumUtilityFeatures);

            if (maximumUtilityFeatures.Count == 0 || maximumUtility <= 0.0)
            {
                var state = CreateState(
                    penaltyUpdate - 1,
                    acceptedMoves,
                    penalties,
                    totalPenaltyIncrements,
                    currentObjective,
                    currentPenaltySum,
                    problem.Sense,
                    parameters.PenaltyWeight);

                return context.Complete(
                    StoppingDecision.Stop(
                        "NoPenalizableFeatures",
                        "No active feature with strictly positive GLS utility remains."),
                    state);
            }

            foreach (TFeature feature in maximumUtilityFeatures)
            {
                penalties.TryGetValue(feature, out int existingPenalty);
                penalties[feature] = checked(existingPenalty + 1);
            }

            currentPenaltySum = checked(
                currentPenaltySum + maximumUtilityFeatures.Count);
            totalPenaltyIncrements = checked(
                totalPenaltyIncrements + maximumUtilityFeatures.Count);

            var penaltyState = CreateState(
                penaltyUpdate,
                acceptedMoves,
                penalties,
                totalPenaltyIncrements,
                currentObjective,
                currentPenaltySum,
                problem.Sense,
                parameters.PenaltyWeight);

            context.CompleteIteration(
                currentObjective,
                penaltyState);

            stop = context.EvaluateStopping(penaltyState);
            if (stop.ShouldStop)
            {
                return context.Complete(stop, penaltyState);
            }

            GuidedDescentResult descent = Descend(
                ref current,
                currentObjective,
                currentPenaltySum,
                penaltyUpdate,
                acceptedMoves,
                totalPenaltyIncrements,
                penalties,
                parameters,
                context,
                solutionCloner,
                cancellationToken);

            currentObjective = descent.Objective;
            currentPenaltySum = descent.PenaltySum;
            acceptedMoves = descent.AcceptedMoves;

            if (descent.StoppingDecision.ShouldStop)
            {
                return context.Complete(descent.StoppingDecision);
            }
        }

        var finalState = CreateState(
            parameters.MaximumPenaltyUpdates,
            acceptedMoves,
            penalties,
            totalPenaltyIncrements,
            currentObjective,
            currentPenaltySum,
            problem.Sense,
            parameters.PenaltyWeight);

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumGuidedPenaltyUpdates",
                "The configured Guided Local Search penalty-update limit was reached."),
            finalState);
    }

    private GuidedDescentResult Descend(
        ref TSolution solution,
        double currentObjective,
        long currentPenaltySum,
        int penaltyUpdates,
        long acceptedMoves,
        long totalPenaltyIncrements,
        Dictionary<TFeature, int> penalties,
        GuidedLocalSearchParameters parameters,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken)
    {
        int acceptedInPhase = 0;

        while (acceptedInPhase < parameters.MaximumAcceptedMovesPerPenaltyPhase)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double currentAugmented = Augment(
                currentObjective,
                currentPenaltySum,
                context.Problem.Sense,
                parameters.PenaltyWeight);

            TMoveEnumerator enumerator = _neighborhood.GetEnumerator(in solution);

            bool selected = false;
            TMove selectedMove = default!;
            double selectedObjective = currentObjective;
            long selectedPenaltySum = currentPenaltySum;
            double selectedAugmented = currentAugmented;

            while (enumerator.MoveNext(out TMove move))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_moveApplicability is not null &&
                    !_moveApplicability.IsApplicable(in solution, in move))
                {
                    continue;
                }

                (double candidateObjective, long candidatePenaltySum) =
                    EvaluateCandidate(
                        ref solution,
                        currentObjective,
                        currentPenaltySum,
                        in move,
                        penalties,
                        context.Problem);

                if (candidatePenaltySum < 0)
                {
                    throw new InvalidOperationException(
                        "A GLS penalty-delta evaluator returned a negative penalty sum.");
                }

                var probeState = CreateState(
                    penaltyUpdates,
                    acceptedMoves,
                    penalties,
                    totalPenaltyIncrements,
                    currentObjective,
                    currentPenaltySum,
                    context.Problem.Sense,
                    parameters.PenaltyWeight);

                long evaluationIndex =
                    context.RegisterExternalProbeEvaluation(
                        candidateObjective,
                        probeState);

                if (context.WouldImprove(candidateObjective))
                {
                    TUndo snapshotUndo =
                        _moveOperator.CaptureUndo(in solution, in move);
                    _moveOperator.Apply(ref solution, in move);

                    TSolution snapshot;
                    try
                    {
                        snapshot = solutionCloner.Clone(solution);
                    }
                    finally
                    {
                        _moveOperator.Undo(
                            ref solution,
                            in move,
                            in snapshotUndo);
                    }

                    context.PromoteOwnedExternalProbeSnapshot(
                        snapshot,
                        candidateObjective,
                        evaluationIndex,
                        probeState);
                }

                StoppingDecision probeStop =
                    context.EvaluateStopping(probeState);
                if (probeStop.ShouldStop)
                {
                    return new GuidedDescentResult(
                        currentObjective,
                        currentPenaltySum,
                        acceptedMoves,
                        localOptimum: false,
                        probeStop);
                }

                if (double.IsNaN(candidateObjective))
                {
                    continue;
                }

                double candidateAugmented = Augment(
                    candidateObjective,
                    candidatePenaltySum,
                    context.Problem.Sense,
                    parameters.PenaltyWeight);

                if (!context.Problem.Sense.IsBetter(
                        candidateAugmented,
                        currentAugmented))
                {
                    continue;
                }

                if (parameters.SelectionPolicy ==
                    LocalSearchSelectionPolicy.FirstImprovement)
                {
                    selected = true;
                    selectedMove = move;
                    selectedObjective = candidateObjective;
                    selectedPenaltySum = candidatePenaltySum;
                    selectedAugmented = candidateAugmented;
                    break;
                }

                if (!selected ||
                    context.Problem.Sense.IsBetter(
                        candidateAugmented,
                        selectedAugmented))
                {
                    selected = true;
                    selectedMove = move;
                    selectedObjective = candidateObjective;
                    selectedPenaltySum = candidatePenaltySum;
                    selectedAugmented = candidateAugmented;
                }
            }

            if (!selected)
            {
                return new GuidedDescentResult(
                    currentObjective,
                    currentPenaltySum,
                    acceptedMoves,
                    localOptimum: true,
                    StoppingDecision.Continue("GuidedLocalOptimum"));
            }

            _moveOperator.Apply(ref solution, in selectedMove);
            currentObjective = selectedObjective;
            currentPenaltySum = selectedPenaltySum;
            acceptedMoves++;
            acceptedInPhase++;

            var state = CreateState(
                penaltyUpdates,
                acceptedMoves,
                penalties,
                totalPenaltyIncrements,
                currentObjective,
                currentPenaltySum,
                context.Problem.Sense,
                parameters.PenaltyWeight);

            context.CompleteIteration(
                currentObjective,
                state);

            StoppingDecision stepStop =
                context.EvaluateStopping(state);
            if (stepStop.ShouldStop)
            {
                return new GuidedDescentResult(
                    currentObjective,
                    currentPenaltySum,
                    acceptedMoves,
                    localOptimum: false,
                    stepStop);
            }
        }

        return new GuidedDescentResult(
            currentObjective,
            currentPenaltySum,
            acceptedMoves,
            localOptimum: false,
            StoppingDecision.Continue(
                "MaximumAcceptedMovesPerPenaltyPhase"));
    }

    private (double Objective, long PenaltySum) EvaluateCandidate(
        ref TSolution solution,
        double currentObjective,
        long currentPenaltySum,
        in TMove move,
        IReadOnlyDictionary<TFeature, int> penalties,
        IOptimizationProblem<TSolution> problem)
    {
        double candidateObjective = default;
        bool hasObjective =
            _objectiveDeltaEvaluator is not null &&
            _objectiveDeltaEvaluator.TryEvaluateCandidateObjective(
                in solution,
                currentObjective,
                in move,
                out candidateObjective);

        long candidatePenaltySum = default;
        bool hasPenaltySum =
            _penaltyDeltaEvaluator is not null &&
            _penaltyDeltaEvaluator.TryEvaluateCandidatePenaltySum(
                in solution,
                currentPenaltySum,
                in move,
                penalties,
                out candidatePenaltySum);

        if (hasObjective && hasPenaltySum)
        {
            return (candidateObjective, candidatePenaltySum);
        }

        TUndo undo =
            _moveOperator.CaptureUndo(in solution, in move);
        _moveOperator.Apply(ref solution, in move);

        try
        {
            if (!hasObjective)
            {
                candidateObjective = problem.Evaluate(solution);
            }

            if (!hasPenaltySum)
            {
                candidatePenaltySum =
                    ComputePenaltySum(in solution, penalties);
            }

            return (candidateObjective, candidatePenaltySum);
        }
        finally
        {
            _moveOperator.Undo(
                ref solution,
                in move,
                in undo);
        }
    }

    private long ComputePenaltySum(
        in TSolution solution,
        IReadOnlyDictionary<TFeature, int> penalties)
    {
        long sum = 0;
        TFeatureEnumerator enumerator =
            _featureModel.GetEnumerator(in solution);

        while (enumerator.MoveNext(out TFeature feature))
        {
            if (penalties.TryGetValue(feature, out int penalty))
            {
                sum = checked(sum + penalty);
            }
        }

        return sum;
    }

    private double FindMaximumUtilityFeatures(
        in TSolution solution,
        IReadOnlyDictionary<TFeature, int> penalties,
        List<TFeature> maximumUtilityFeatures)
    {
        double maximumUtility = double.NegativeInfinity;
        TFeatureEnumerator enumerator =
            _featureModel.GetEnumerator(in solution);

        while (enumerator.MoveNext(out TFeature feature))
        {
            double cost =
                _featureModel.GetFeatureCost(in solution, in feature);

            if (!double.IsFinite(cost) || cost < 0.0)
            {
                throw new InvalidOperationException(
                    "GLS feature costs must be finite and non-negative.");
            }

            penalties.TryGetValue(feature, out int penalty);
            double utility = cost / (1.0 + penalty);

            if (utility > maximumUtility)
            {
                maximumUtility = utility;
                maximumUtilityFeatures.Clear();
                maximumUtilityFeatures.Add(feature);
            }
            else if (utility.Equals(maximumUtility))
            {
                maximumUtilityFeatures.Add(feature);
            }
        }

        return maximumUtility;
    }

    private static double Augment(
        double objective,
        long penaltySum,
        OptimizationSense sense,
        double penaltyWeight)
    {
        double scaledPenalty = penaltyWeight * penaltySum;

        return sense == OptimizationSense.Minimize
            ? objective + scaledPenalty
            : objective - scaledPenalty;
    }

    private static GuidedLocalSearchState CreateState(
        int penaltyUpdates,
        long acceptedMoves,
        IReadOnlyDictionary<TFeature, int> penalties,
        long totalPenaltyIncrements,
        double currentObjective,
        long currentPenaltySum,
        OptimizationSense sense,
        double penaltyWeight) =>
        new(
            penaltyUpdates,
            acceptedMoves,
            penalties.Count,
            totalPenaltyIncrements,
            currentObjective,
            Augment(
                currentObjective,
                currentPenaltySum,
                sense,
                penaltyWeight));

    private readonly struct GuidedDescentResult
    {
        public GuidedDescentResult(
            double objective,
            long penaltySum,
            long acceptedMoves,
            bool localOptimum,
            StoppingDecision stoppingDecision)
        {
            Objective = objective;
            PenaltySum = penaltySum;
            AcceptedMoves = acceptedMoves;
            IsLocalOptimum = localOptimum;
            StoppingDecision = stoppingDecision;
        }

        public double Objective { get; }

        public long PenaltySum { get; }

        public long AcceptedMoves { get; }

        public bool IsLocalOptimum { get; }

        public StoppingDecision StoppingDecision { get; }
    }
}
