using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Trajectory;

/// <summary>
/// Executes one trajectory move using a reversible move operator.
/// </summary>
/// <remarks>
/// Fast path:
/// if an exact move objective evaluator is available, candidate acceptance is decided
/// before mutation. Rejected moves therefore require neither Apply nor Undo.
///
/// Fallback path:
/// capture undo, apply, fully evaluate, and undo when rejected.
/// The executor also attempts to undo if evaluation or acceptance throws.
/// </remarks>
public sealed class ReversibleTrajectoryStepExecutor<
    TSolution,
    TMove,
    TUndo>
{
    private readonly IReversibleMoveOperator<
        TSolution,
        TMove,
        TUndo> _moveOperator;

    private readonly TrajectoryObjectiveEvaluator<TSolution>
        _objectiveEvaluator;

    private readonly ITrajectoryAcceptancePolicy
        _acceptancePolicy;

    private readonly IMoveObjectiveDeltaEvaluator<
        TSolution,
        TMove>? _deltaEvaluator;

    public ReversibleTrajectoryStepExecutor(
        IReversibleMoveOperator<
            TSolution,
            TMove,
            TUndo> moveOperator,
        TrajectoryObjectiveEvaluator<TSolution>
            objectiveEvaluator,
        ITrajectoryAcceptancePolicy
            acceptancePolicy,
        IMoveObjectiveDeltaEvaluator<
            TSolution,
            TMove>? deltaEvaluator = null)
    {
        _moveOperator =
            moveOperator ??
            throw new ArgumentNullException(
                nameof(moveOperator));

        _objectiveEvaluator =
            objectiveEvaluator ??
            throw new ArgumentNullException(
                nameof(objectiveEvaluator));

        _acceptancePolicy =
            acceptancePolicy ??
            throw new ArgumentNullException(
                nameof(acceptancePolicy));

        _deltaEvaluator =
            deltaEvaluator;
    }

    public TrajectoryStepResult Execute(
        ref TSolution solution,
        double currentObjective,
        double bestObjective,
        in TMove move,
        long iteration,
        OptimizationSense sense,
        IRandomSource random,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(random);

        cancellationToken.ThrowIfCancellationRequested();

        if (_deltaEvaluator is not null &&
            _deltaEvaluator.TryEvaluateCandidateObjective(
                in solution,
                currentObjective,
                in move,
                out double deltaCandidateObjective))
        {
            var deltaContext =
                new TrajectoryAcceptanceContext(
                    sense,
                    iteration,
                    currentObjective,
                    deltaCandidateObjective,
                    bestObjective);

            bool accepted =
                _acceptancePolicy.ShouldAccept(
                    in deltaContext,
                    random);

            if (accepted)
            {
                _moveOperator.Apply(
                    ref solution,
                    in move);
            }

            return
                new TrajectoryStepResult(
                    Accepted: accepted,
                    UsedDeltaEvaluation: true,
                    MoveApplied: accepted,
                    MoveUndone: false,
                    PreviousObjective:
                        currentObjective,
                    CandidateObjective:
                        deltaCandidateObjective,
                    ResultingObjective:
                        accepted
                            ? deltaCandidateObjective
                            : currentObjective,
                    Quality:
                        deltaContext.Quality);
        }

        TUndo undo =
            _moveOperator.CaptureUndo(
                in solution,
                in move);

        _moveOperator.Apply(
            ref solution,
            in move);

        bool keepAppliedMove = false;
        bool undoAttempted = false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            double candidateObjective =
                _objectiveEvaluator(
                    in solution);

            var fullContext =
                new TrajectoryAcceptanceContext(
                    sense,
                    iteration,
                    currentObjective,
                    candidateObjective,
                    bestObjective);

            bool accepted =
                _acceptancePolicy.ShouldAccept(
                    in fullContext,
                    random);

            if (accepted)
            {
                keepAppliedMove = true;
            }

            if (!accepted)
            {
                _moveOperator.Undo(
                    ref solution,
                    in move,
                    in undo);

                undoAttempted = true;
            }

            return
                new TrajectoryStepResult(
                    Accepted: accepted,
                    UsedDeltaEvaluation: false,
                    MoveApplied: true,
                    MoveUndone: !accepted,
                    PreviousObjective:
                        currentObjective,
                    CandidateObjective:
                        candidateObjective,
                    ResultingObjective:
                        accepted
                            ? candidateObjective
                            : currentObjective,
                    Quality:
                        fullContext.Quality);
        }
        finally
        {
            if (!keepAppliedMove &&
                !undoAttempted)
            {
                _moveOperator.Undo(
                    ref solution,
                    in move,
                    in undo);
            }
        }
    }
}