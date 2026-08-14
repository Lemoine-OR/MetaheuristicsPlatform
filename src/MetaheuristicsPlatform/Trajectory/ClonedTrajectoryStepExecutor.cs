using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Trajectory;

/// <summary>
/// Generic fallback executor for moves that cannot efficiently undo themselves.
/// </summary>
/// <remarks>
/// With an exact delta evaluator, rejected moves do not clone the solution.
/// Accepted moves clone once, apply to the clone, then replace the current solution.
///
/// Without a delta evaluator, one candidate clone is created, transformed, evaluated,
/// and retained only if accepted.
/// </remarks>
public sealed class ClonedTrajectoryStepExecutor<
    TSolution,
    TMove>
{
    private readonly IMoveOperator<
        TSolution,
        TMove> _moveOperator;

    private readonly TrajectorySolutionCloner<TSolution>
        _solutionCloner;

    private readonly TrajectoryObjectiveEvaluator<TSolution>
        _objectiveEvaluator;

    private readonly ITrajectoryAcceptancePolicy
        _acceptancePolicy;

    private readonly IMoveObjectiveDeltaEvaluator<
        TSolution,
        TMove>? _deltaEvaluator;

    public ClonedTrajectoryStepExecutor(
        IMoveOperator<TSolution, TMove>
            moveOperator,
        TrajectorySolutionCloner<TSolution>
            solutionCloner,
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

        _solutionCloner =
            solutionCloner ??
            throw new ArgumentNullException(
                nameof(solutionCloner));

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
                TSolution acceptedCandidate =
                    _solutionCloner(
                        in solution);

                _moveOperator.Apply(
                    ref acceptedCandidate,
                    in move);

                solution =
                    acceptedCandidate;
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

        TSolution candidate =
            _solutionCloner(
                in solution);

        _moveOperator.Apply(
            ref candidate,
            in move);

        cancellationToken.ThrowIfCancellationRequested();

        double candidateObjective =
            _objectiveEvaluator(
                in candidate);

        var fullContext =
            new TrajectoryAcceptanceContext(
                sense,
                iteration,
                currentObjective,
                candidateObjective,
                bestObjective);

        bool fullAccepted =
            _acceptancePolicy.ShouldAccept(
                in fullContext,
                random);

        if (fullAccepted)
        {
            solution =
                candidate;
        }

        return
            new TrajectoryStepResult(
                Accepted: fullAccepted,
                UsedDeltaEvaluation: false,
                MoveApplied: true,
                MoveUndone: false,
                PreviousObjective:
                    currentObjective,
                CandidateObjective:
                    candidateObjective,
                ResultingObjective:
                    fullAccepted
                        ? candidateObjective
                        : currentObjective,
                Quality:
                    fullContext.Quality);
    }
}