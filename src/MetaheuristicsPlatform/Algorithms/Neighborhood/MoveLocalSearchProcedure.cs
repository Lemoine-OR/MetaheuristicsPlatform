using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Allocation-free move local search supporting first- and best-improvement selection.
/// Candidate objectives use exact deltas when available and reversible apply/evaluate/undo otherwise.
/// </summary>
public sealed class MoveLocalSearchProcedure<
    TSolution,
    TMove,
    TUndo,
    TEnumerator> : ILocalSearchProcedure<TSolution>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    private readonly IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> _neighborhood;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo> _moveOperator;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>? _deltaEvaluator;
    private readonly IMoveApplicability<TSolution, TMove>? _moveApplicability;
    private readonly LocalSearchSelectionPolicy _selectionPolicy;
    private readonly int _maximumAcceptedMoves;

    public MoveLocalSearchProcedure(
        IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        LocalSearchSelectionPolicy selectionPolicy,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null,
        IMoveApplicability<TSolution, TMove>? moveApplicability = null,
        int maximumAcceptedMoves = int.MaxValue)
    {
        _neighborhood = neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        _moveOperator = moveOperator ?? throw new ArgumentNullException(nameof(moveOperator));

        if (!Enum.IsDefined(selectionPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(selectionPolicy));
        }

        if (maximumAcceptedMoves <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAcceptedMoves));
        }

        _selectionPolicy = selectionPolicy;
        _deltaEvaluator = deltaEvaluator;
        _moveApplicability = moveApplicability;
        _maximumAcceptedMoves = maximumAcceptedMoves;
    }

    public LocalSearchProcedureResult Improve(
        ref TSolution solution,
        double currentFitness,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        long acceptedMoves = 0;

        while (acceptedMoves < _maximumAcceptedMoves)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TEnumerator enumerator = _neighborhood.GetEnumerator(in solution);
            bool selected = false;
            TMove selectedMove = default!;
            double selectedFitness = currentFitness;
            long selectedEvaluationIndex = 0;

            while (enumerator.MoveNext(out TMove move))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_moveApplicability is not null &&
                    !_moveApplicability.IsApplicable(in solution, in move))
                {
                    continue;
                }

                double candidateFitness = EvaluateCandidate(
                    context.Problem,
                    ref solution,
                    currentFitness,
                    in move);

                long evaluationIndex =
                    context.RegisterExternalProbeEvaluation(candidateFitness);

                StoppingDecision probeStop = context.EvaluateStopping();
                if (probeStop.ShouldStop)
                {
                    return new LocalSearchProcedureResult(
                        currentFitness,
                        acceptedMoves,
                        localOptimum: false,
                        probeStop);
                }

                if (!context.Problem.Sense.IsBetter(candidateFitness, currentFitness))
                {
                    continue;
                }

                if (_selectionPolicy == LocalSearchSelectionPolicy.FirstImprovement)
                {
                    selected = true;
                    selectedMove = move;
                    selectedFitness = candidateFitness;
                    selectedEvaluationIndex = evaluationIndex;
                    break;
                }

                if (!selected ||
                    context.Problem.Sense.IsBetter(candidateFitness, selectedFitness))
                {
                    selected = true;
                    selectedMove = move;
                    selectedFitness = candidateFitness;
                    selectedEvaluationIndex = evaluationIndex;
                }
            }

            if (!selected)
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    acceptedMoves,
                    localOptimum: true,
                    StoppingDecision.Continue("LocalOptimum"));
            }

            _moveOperator.Apply(ref solution, in selectedMove);
            currentFitness = selectedFitness;
            acceptedMoves++;

            if (context.WouldImprove(currentFitness))
            {
                TSolution snapshot = solutionCloner.Clone(solution);
                context.PromoteOwnedExternalProbeSnapshot(
                    snapshot,
                    currentFitness,
                    selectedEvaluationIndex);
            }

            context.CompleteIteration(currentFitness);

            StoppingDecision stepStop = context.EvaluateStopping();
            if (stepStop.ShouldStop)
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    acceptedMoves,
                    localOptimum: false,
                    stepStop);
            }
        }

        return new LocalSearchProcedureResult(
            currentFitness,
            acceptedMoves,
            localOptimum: false,
            StoppingDecision.Continue("MaximumAcceptedMoves"));
    }

    private double EvaluateCandidate(
        IOptimizationProblem<TSolution> problem,
        ref TSolution solution,
        double currentFitness,
        in TMove move)
    {
        if (_deltaEvaluator is not null &&
            _deltaEvaluator.TryEvaluateCandidateObjective(
                in solution,
                currentFitness,
                in move,
                out double candidateFitness))
        {
            return candidateFitness;
        }

        TUndo undo = _moveOperator.CaptureUndo(in solution, in move);
        _moveOperator.Apply(ref solution, in move);

        try
        {
            return problem.Evaluate(solution);
        }
        finally
        {
            _moveOperator.Undo(ref solution, in move, in undo);
        }
    }
}
