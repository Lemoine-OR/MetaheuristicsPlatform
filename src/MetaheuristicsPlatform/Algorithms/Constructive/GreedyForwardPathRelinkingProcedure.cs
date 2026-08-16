using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Greedy forward path relinking between an initiating solution and a guiding solution.
/// At each path position, all target-directed moves are probed and the best objective move
/// is selected. Objective deltas are used when an exact evaluator is supplied; otherwise
/// candidates are evaluated through reversible apply/evaluate/undo.
/// </summary>
public sealed class GreedyForwardPathRelinkingProcedure<
    TSolution,
    TMove,
    TUndo,
    TEnumerator> : IPathRelinkingProcedure<TSolution>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    private readonly IPathRelinkingNeighborhood<TSolution, TMove, TEnumerator> _neighborhood;
    private readonly IPathRelinkingDistance<TSolution> _distance;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo> _moveOperator;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>? _deltaEvaluator;

    /// <summary>Creates the canonical greedy forward path-relinking procedure.</summary>
    public GreedyForwardPathRelinkingProcedure(
        IPathRelinkingNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IPathRelinkingDistance<TSolution> distance,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null)
    {
        _neighborhood =
            neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        _distance =
            distance ?? throw new ArgumentNullException(nameof(distance));
        _moveOperator =
            moveOperator ?? throw new ArgumentNullException(nameof(moveOperator));
        _deltaEvaluator = deltaEvaluator;
    }

    /// <inheritdoc />
    public PathRelinkingProcedureResult<TSolution> Relink(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        if (double.IsNaN(initiatingFitness))
        {
            throw new ArgumentOutOfRangeException(nameof(initiatingFitness));
        }

        if (maximumPathSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPathSteps));
        }

        TSolution current =
            solutionCloner.Clone(initiatingSolution);

        double currentFitness =
            initiatingFitness;

        TSolution best =
            solutionCloner.Clone(initiatingSolution);

        double bestFitness =
            initiatingFitness;

        int currentDistance =
            GetValidatedDistance(
                in current,
                in guidingSolution,
                context.Problem);

        int pathSteps = 0;
        long candidateEvaluations = 0;

        while (currentDistance > 0 &&
               pathSteps < maximumPathSteps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TEnumerator enumerator =
                _neighborhood.GetEnumerator(
                    in current,
                    in guidingSolution,
                    context.Problem);

            bool selected = false;
            TMove selectedMove = default!;
            double selectedFitness = currentFitness;
            long selectedEvaluationIndex = 0;

            while (enumerator.MoveNext(out TMove move))
            {
                cancellationToken.ThrowIfCancellationRequested();

                double candidateFitness =
                    EvaluateCandidate(
                        context.Problem,
                        ref current,
                        currentFitness,
                        in move);

                long evaluationIndex =
                    context.RegisterExternalProbeEvaluation(
                        candidateFitness);

                candidateEvaluations++;

                StoppingDecision probeStop =
                    context.EvaluateStopping();

                if (probeStop.ShouldStop)
                {
                    return new PathRelinkingProcedureResult<TSolution>(
                        best,
                        bestFitness,
                        pathSteps,
                        candidateEvaluations,
                        ReachedGuidingSolution: false,
                        probeStop);
                }

                if (!selected ||
                    context.Problem.Sense.IsBetter(
                        candidateFitness,
                        selectedFitness))
                {
                    selected = true;
                    selectedMove = move;
                    selectedFitness = candidateFitness;
                    selectedEvaluationIndex = evaluationIndex;
                }
            }

            if (!selected)
            {
                throw new InvalidOperationException(
                    "Path relinking has positive remaining distance but the domain " +
                    "neighborhood exposed no target-directed move.");
            }

            _moveOperator.Apply(
                ref current,
                in selectedMove);

            int nextDistance =
                GetValidatedDistance(
                    in current,
                    in guidingSolution,
                    context.Problem);

            if (nextDistance >= currentDistance)
            {
                throw new InvalidOperationException(
                    "The selected path-relinking move did not strictly decrease " +
                    "the distance to the guiding solution.");
            }

            currentDistance = nextDistance;
            currentFitness = selectedFitness;
            pathSteps++;

            if (context.WouldImprove(currentFitness))
            {
                TSolution globalSnapshot =
                    solutionCloner.Clone(current);

                context.PromoteOwnedExternalProbeSnapshot(
                    globalSnapshot,
                    currentFitness,
                    selectedEvaluationIndex);
            }

            if (context.Problem.Sense.IsBetter(
                    currentFitness,
                    bestFitness))
            {
                best =
                    solutionCloner.Clone(current);
                bestFitness =
                    currentFitness;
            }

            StoppingDecision stepStop =
                context.EvaluateStopping();

            if (stepStop.ShouldStop)
            {
                return new PathRelinkingProcedureResult<TSolution>(
                    best,
                    bestFitness,
                    pathSteps,
                    candidateEvaluations,
                    currentDistance == 0,
                    stepStop);
            }
        }

        return new PathRelinkingProcedureResult<TSolution>(
            best,
            bestFitness,
            pathSteps,
            candidateEvaluations,
            currentDistance == 0,
            StoppingDecision.Continue(
                currentDistance == 0
                    ? "PathRelinkingReachedGuide"
                    : "MaximumPathSteps"));
    }

    private int GetValidatedDistance(
        in TSolution first,
        in TSolution second,
        IOptimizationProblem<TSolution> problem)
    {
        int distance =
            _distance.GetDistance(
                in first,
                in second,
                problem);

        if (distance < 0)
        {
            throw new InvalidOperationException(
                "Path-relinking distance must be non-negative.");
        }

        return distance;
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
            if (double.IsNaN(candidateFitness))
            {
                throw new InvalidOperationException(
                    "The path-relinking delta evaluator returned NaN.");
            }

            return candidateFitness;
        }

        TUndo undo =
            _moveOperator.CaptureUndo(
                in solution,
                in move);

        _moveOperator.Apply(
            ref solution,
            in move);

        try
        {
            double evaluatedFitness =
                problem.Evaluate(solution);

            if (double.IsNaN(evaluatedFitness))
            {
                throw new InvalidOperationException(
                    "The objective returned NaN during path relinking.");
            }

            return evaluatedFitness;
        }
        finally
        {
            _moveOperator.Undo(
                ref solution,
                in move,
                in undo);
        }
    }
}