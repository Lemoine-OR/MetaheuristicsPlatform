using System.Buffers;
using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Literature-backed advanced path-relinking engine supporting forward, backward,
/// back-and-forward and mixed directions, orthogonal path truncation, and canonical
/// greedy-randomized adaptive restricted candidate lists.
/// </summary>
public sealed class AdvancedPathRelinkingProcedure<
    TSolution,
    TMove,
    TUndo,
    TEnumerator> : IAdvancedPathRelinkingProcedure<TSolution>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    private readonly IPathRelinkingNeighborhood<TSolution, TMove, TEnumerator> _neighborhood;
    private readonly IPathRelinkingDistance<TSolution> _distance;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo> _moveOperator;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>? _deltaEvaluator;

    /// <summary>Creates an advanced path-relinking engine.</summary>
    public AdvancedPathRelinkingProcedure(
        IPathRelinkingNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IPathRelinkingDistance<TSolution> distance,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null)
    {
        _neighborhood = neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        _distance = distance ?? throw new ArgumentNullException(nameof(distance));
        _moveOperator = moveOperator ?? throw new ArgumentNullException(nameof(moveOperator));
        _deltaEvaluator = deltaEvaluator;
    }

    /// <summary>
    /// Compatibility entry point preserving the v0.30.x greedy-forward behavior.
    /// </summary>
    public PathRelinkingProcedureResult<TSolution> Relink(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken)
    {
        ValidateCommon(initiatingFitness, context, solutionCloner, maximumPathSteps);

        return RunDirectional(
            in initiatingSolution,
            initiatingFitness,
            in guidingSolution,
            PathRelinkingMoveSelectionStrategy.Greedy,
            greedyRandomizedAlpha: 0.0,
            pathFraction: 1.0,
            context,
            solutionCloner,
            maximumPathSteps,
            cancellationToken);
    }

    /// <inheritdoc />
    public PathRelinkingProcedureResult<TSolution> RelinkAdvanced(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        double guidingFitness,
        PathRelinkingExecutionOptions executionOptions,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken)
    {
        ValidateCommon(initiatingFitness, context, solutionCloner, maximumPathSteps);

        if (double.IsNaN(guidingFitness))
        {
            throw new ArgumentOutOfRangeException(nameof(guidingFitness));
        }

        executionOptions.Validate();

        return executionOptions.Direction switch
        {
            PathRelinkingDirectionStrategy.Forward =>
                RunDirectional(
                    in initiatingSolution,
                    initiatingFitness,
                    in guidingSolution,
                    executionOptions.MoveSelection,
                    executionOptions.GreedyRandomizedAlpha,
                    executionOptions.PathFraction,
                    context,
                    solutionCloner,
                    maximumPathSteps,
                    cancellationToken),

            PathRelinkingDirectionStrategy.Backward =>
                RunDirectional(
                    in guidingSolution,
                    guidingFitness,
                    in initiatingSolution,
                    executionOptions.MoveSelection,
                    executionOptions.GreedyRandomizedAlpha,
                    executionOptions.PathFraction,
                    context,
                    solutionCloner,
                    maximumPathSteps,
                    cancellationToken),

            PathRelinkingDirectionStrategy.BackAndForward =>
                RunBackAndForward(
                    in initiatingSolution,
                    initiatingFitness,
                    in guidingSolution,
                    guidingFitness,
                    executionOptions,
                    context,
                    solutionCloner,
                    maximumPathSteps,
                    cancellationToken),

            PathRelinkingDirectionStrategy.Mixed =>
                RunMixed(
                    in initiatingSolution,
                    initiatingFitness,
                    in guidingSolution,
                    guidingFitness,
                    executionOptions,
                    context,
                    solutionCloner,
                    maximumPathSteps,
                    cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(executionOptions))
        };
    }

    private PathRelinkingProcedureResult<TSolution> RunBackAndForward(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        double guidingFitness,
        PathRelinkingExecutionOptions options,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken)
    {
        PathRelinkingProcedureResult<TSolution> backward =
            RunDirectional(
                in guidingSolution,
                guidingFitness,
                in initiatingSolution,
                options.MoveSelection,
                options.GreedyRandomizedAlpha,
                options.PathFraction,
                context,
                solutionCloner,
                maximumPathSteps,
                cancellationToken);

        if (backward.StoppingDecision.ShouldStop)
        {
            return backward;
        }

        PathRelinkingProcedureResult<TSolution> forward =
            RunDirectional(
                in initiatingSolution,
                initiatingFitness,
                in guidingSolution,
                options.MoveSelection,
                options.GreedyRandomizedAlpha,
                options.PathFraction,
                context,
                solutionCloner,
                maximumPathSteps,
                cancellationToken);

        TSolution bestSolution;
        double bestFitness;

        if (context.Problem.Sense.IsBetter(backward.BestFitness, forward.BestFitness))
        {
            bestSolution = backward.BestSolution;
            bestFitness = backward.BestFitness;
        }
        else
        {
            bestSolution = forward.BestSolution;
            bestFitness = forward.BestFitness;
        }

        return new PathRelinkingProcedureResult<TSolution>(
            bestSolution,
            bestFitness,
            backward.PathSteps + forward.PathSteps,
            backward.CandidateEvaluations + forward.CandidateEvaluations,
            backward.ReachedGuidingSolution && forward.ReachedGuidingSolution,
            forward.StoppingDecision.ShouldStop
                ? forward.StoppingDecision
                : StoppingDecision.Continue("BackAndForwardPathRelinkingCompleted"));
    }

    private PathRelinkingProcedureResult<TSolution> RunDirectional(
        in TSolution startSolution,
        double startFitness,
        in TSolution targetSolution,
        PathRelinkingMoveSelectionStrategy moveSelection,
        double greedyRandomizedAlpha,
        double pathFraction,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken)
    {
        TSolution current = solutionCloner.Clone(startSolution);
        double currentFitness = startFitness;
        TSolution best = solutionCloner.Clone(startSolution);
        double bestFitness = startFitness;

        int currentDistance =
            GetValidatedDistance(in current, in targetSolution, context.Problem);
        int initialDistance = currentDistance;
        int requiredReduction = RequiredReduction(initialDistance, pathFraction);

        int pathSteps = 0;
        long candidateEvaluations = 0;

        while (currentDistance > 0 &&
               pathSteps < maximumPathSteps &&
               initialDistance - currentDistance < requiredReduction)
        {
            cancellationToken.ThrowIfCancellationRequested();

            MoveSelectionResult selected =
                SelectMove(
                    ref current,
                    currentFitness,
                    in targetSolution,
                    moveSelection,
                    greedyRandomizedAlpha,
                    context,
                    cancellationToken);

            candidateEvaluations += selected.CandidateEvaluations;

            if (selected.StoppingDecision.ShouldStop)
            {
                return new PathRelinkingProcedureResult<TSolution>(
                    best,
                    bestFitness,
                    pathSteps,
                    candidateEvaluations,
                    false,
                    selected.StoppingDecision);
            }

            if (!selected.Selected)
            {
                throw new InvalidOperationException(
                    "Path relinking has positive remaining distance but the domain " +
                    "neighborhood exposed no target-directed move.");
            }

            TMove selectedMove = selected.Move;

            _moveOperator.Apply(ref current, in selectedMove);

            int nextDistance =
                GetValidatedDistance(in current, in targetSolution, context.Problem);

            if (nextDistance >= currentDistance)
            {
                throw new InvalidOperationException(
                    "The selected path-relinking move did not strictly decrease " +
                    "the distance to the guiding solution.");
            }

            currentDistance = nextDistance;
            currentFitness = selected.Fitness;
            pathSteps++;

            PromoteVisitedProbe(
                in current,
                currentFitness,
                selected.EvaluationIndex,
                context,
                solutionCloner);

            if (context.Problem.Sense.IsBetter(currentFitness, bestFitness))
            {
                best = solutionCloner.Clone(current);
                bestFitness = currentFitness;
            }

            StoppingDecision stop = context.EvaluateStopping();
            if (stop.ShouldStop)
            {
                return new PathRelinkingProcedureResult<TSolution>(
                    best,
                    bestFitness,
                    pathSteps,
                    candidateEvaluations,
                    currentDistance == 0,
                    stop);
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
                    : initialDistance - currentDistance >= requiredReduction
                        ? "PathRelinkingTruncatedFractionCompleted"
                        : "MaximumPathSteps"));
    }

    private PathRelinkingProcedureResult<TSolution> RunMixed(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        double guidingFitness,
        PathRelinkingExecutionOptions options,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken)
    {
        TSolution left = solutionCloner.Clone(initiatingSolution);
        TSolution right = solutionCloner.Clone(guidingSolution);
        double leftFitness = initiatingFitness;
        double rightFitness = guidingFitness;

        TSolution best;
        double bestFitness;

        if (context.Problem.Sense.IsBetter(leftFitness, rightFitness))
        {
            best = solutionCloner.Clone(left);
            bestFitness = leftFitness;
        }
        else
        {
            best = solutionCloner.Clone(right);
            bestFitness = rightFitness;
        }

        int currentDistance =
            GetValidatedDistance(in left, in right, context.Problem);
        int initialDistance = currentDistance;
        int requiredReduction = RequiredReduction(initialDistance, options.PathFraction);

        int pathSteps = 0;
        long candidateEvaluations = 0;
        bool advanceLeft = true;

        while (currentDistance > 0 &&
               pathSteps < maximumPathSteps &&
               initialDistance - currentDistance < requiredReduction)
        {
            cancellationToken.ThrowIfCancellationRequested();

            MoveSelectionResult selected;

            if (advanceLeft)
            {
                selected = SelectMove(
                    ref left,
                    leftFitness,
                    in right,
                    options.MoveSelection,
                    options.GreedyRandomizedAlpha,
                    context,
                    cancellationToken);
            }
            else
            {
                selected = SelectMove(
                    ref right,
                    rightFitness,
                    in left,
                    options.MoveSelection,
                    options.GreedyRandomizedAlpha,
                    context,
                    cancellationToken);
            }

            candidateEvaluations += selected.CandidateEvaluations;

            if (selected.StoppingDecision.ShouldStop)
            {
                return new PathRelinkingProcedureResult<TSolution>(
                    best,
                    bestFitness,
                    pathSteps,
                    candidateEvaluations,
                    false,
                    selected.StoppingDecision);
            }

            if (!selected.Selected)
            {
                throw new InvalidOperationException(
                    "Mixed path relinking has positive remaining endpoint distance but " +
                    "the active endpoint exposed no target-directed move.");
            }

            TMove selectedMove = selected.Move;

            if (advanceLeft)
            {
                _moveOperator.Apply(ref left, in selectedMove);
                leftFitness = selected.Fitness;
            }
            else
            {
                _moveOperator.Apply(ref right, in selectedMove);
                rightFitness = selected.Fitness;
            }

            int nextDistance =
                GetValidatedDistance(in left, in right, context.Problem);

            if (nextDistance >= currentDistance)
            {
                throw new InvalidOperationException(
                    "The selected mixed path-relinking move did not strictly decrease " +
                    "the distance between active endpoints.");
            }

            currentDistance = nextDistance;
            pathSteps++;

            if (advanceLeft)
            {
                PromoteVisitedProbe(
                    in left,
                    leftFitness,
                    selected.EvaluationIndex,
                    context,
                    solutionCloner);

                if (context.Problem.Sense.IsBetter(leftFitness, bestFitness))
                {
                    best = solutionCloner.Clone(left);
                    bestFitness = leftFitness;
                }
            }
            else
            {
                PromoteVisitedProbe(
                    in right,
                    rightFitness,
                    selected.EvaluationIndex,
                    context,
                    solutionCloner);

                if (context.Problem.Sense.IsBetter(rightFitness, bestFitness))
                {
                    best = solutionCloner.Clone(right);
                    bestFitness = rightFitness;
                }
            }

            advanceLeft = !advanceLeft;

            StoppingDecision stop = context.EvaluateStopping();
            if (stop.ShouldStop)
            {
                return new PathRelinkingProcedureResult<TSolution>(
                    best,
                    bestFitness,
                    pathSteps,
                    candidateEvaluations,
                    currentDistance == 0,
                    stop);
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
                    ? "MixedPathRelinkingEndpointsMet"
                    : initialDistance - currentDistance >= requiredReduction
                        ? "MixedPathRelinkingTruncatedFractionCompleted"
                        : "MaximumPathSteps"));
    }

    private MoveSelectionResult SelectMove(
        ref TSolution current,
        double currentFitness,
        in TSolution target,
        PathRelinkingMoveSelectionStrategy selection,
        double alpha,
        OptimizationContext<TSolution> context,
        CancellationToken cancellationToken) =>
        selection switch
        {
            PathRelinkingMoveSelectionStrategy.Greedy =>
                SelectGreedyMove(
                    ref current,
                    currentFitness,
                    in target,
                    context,
                    cancellationToken),

            PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive =>
                SelectGreedyRandomizedMove(
                    ref current,
                    currentFitness,
                    in target,
                    alpha,
                    context,
                    cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(selection))
        };

    private MoveSelectionResult SelectGreedyMove(
        ref TSolution current,
        double currentFitness,
        in TSolution target,
        OptimizationContext<TSolution> context,
        CancellationToken cancellationToken)
    {
        TEnumerator enumerator =
            _neighborhood.GetEnumerator(in current, in target, context.Problem);

        bool selected = false;
        TMove selectedMove = default!;
        double selectedFitness = currentFitness;
        long selectedEvaluationIndex = 0;
        long candidateEvaluations = 0;

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
                context.RegisterExternalProbeEvaluation(candidateFitness);

            candidateEvaluations++;

            StoppingDecision probeStop = context.EvaluateStopping();
            if (probeStop.ShouldStop)
            {
                return new MoveSelectionResult(
                    false,
                    default!,
                    currentFitness,
                    0,
                    candidateEvaluations,
                    probeStop);
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

        return new MoveSelectionResult(
            selected,
            selectedMove,
            selectedFitness,
            selectedEvaluationIndex,
            candidateEvaluations,
            StoppingDecision.Continue("PathRelinkingMoveSelected"));
    }

    private MoveSelectionResult SelectGreedyRandomizedMove(
        ref TSolution current,
        double currentFitness,
        in TSolution target,
        double alpha,
        OptimizationContext<TSolution> context,
        CancellationToken cancellationToken)
    {
        CandidateProbe[] buffer =
            ArrayPool<CandidateProbe>.Shared.Rent(16);

        int count = 0;
        double best = 0.0;
        double worst = 0.0;
        long candidateEvaluations = 0;

        try
        {
            TEnumerator enumerator =
                _neighborhood.GetEnumerator(in current, in target, context.Problem);

            while (enumerator.MoveNext(out TMove move))
            {
                cancellationToken.ThrowIfCancellationRequested();

                double candidateFitness =
                    EvaluateCandidate(
                        context.Problem,
                        ref current,
                        currentFitness,
                        in move);

                if (!double.IsFinite(candidateFitness))
                {
                    throw new InvalidOperationException(
                        "Greedy-randomized path relinking requires finite candidate objectives.");
                }

                long evaluationIndex =
                    context.RegisterExternalProbeEvaluation(candidateFitness);

                candidateEvaluations++;

                StoppingDecision probeStop = context.EvaluateStopping();
                if (probeStop.ShouldStop)
                {
                    return new MoveSelectionResult(
                        false,
                        default!,
                        currentFitness,
                        0,
                        candidateEvaluations,
                        probeStop);
                }

                if (count == buffer.Length)
                {
                    CandidateProbe[] larger =
                        ArrayPool<CandidateProbe>.Shared.Rent(buffer.Length * 2);

                    Array.Copy(buffer, larger, count);
                    ReturnBuffer(buffer);
                    buffer = larger;
                }

                buffer[count++] =
                    new CandidateProbe(move, candidateFitness, evaluationIndex);

                if (count == 1)
                {
                    best = candidateFitness;
                    worst = candidateFitness;
                }
                else
                {
                    if (context.Problem.Sense.IsBetter(candidateFitness, best))
                    {
                        best = candidateFitness;
                    }

                    if (context.Problem.Sense.IsBetter(worst, candidateFitness))
                    {
                        worst = candidateFitness;
                    }
                }
            }

            if (count == 0)
            {
                return new MoveSelectionResult(
                    false,
                    default!,
                    currentFitness,
                    0,
                    candidateEvaluations,
                    StoppingDecision.Continue("NoTargetDirectedMove"));
            }

            double threshold =
                context.Problem.Sense == OptimizationSense.Minimize
                    ? best + alpha * (worst - best)
                    : best - alpha * (best - worst);

            int eligibleCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (IsEligible(buffer[i].Fitness, threshold, context.Problem.Sense))
                {
                    eligibleCount++;
                }
            }

            if (eligibleCount <= 0)
            {
                throw new InvalidOperationException(
                    "The greedy-randomized path-relinking restricted candidate list is empty.");
            }

            int selectedOrdinal = context.Random.NextInt32(eligibleCount);

            for (int i = 0; i < count; i++)
            {
                CandidateProbe probe = buffer[i];
                if (!IsEligible(probe.Fitness, threshold, context.Problem.Sense))
                {
                    continue;
                }

                if (selectedOrdinal == 0)
                {
                    return new MoveSelectionResult(
                        true,
                        probe.Move,
                        probe.Fitness,
                        probe.EvaluationIndex,
                        candidateEvaluations,
                        StoppingDecision.Continue("GreedyRandomizedPathMoveSelected"));
                }

                selectedOrdinal--;
            }

            throw new InvalidOperationException(
                "Unable to select a greedy-randomized path-relinking move.");
        }
        finally
        {
            ReturnBuffer(buffer);
        }
    }

    private static bool IsEligible(
        double fitness,
        double threshold,
        OptimizationSense sense) =>
        sense == OptimizationSense.Minimize
            ? fitness <= threshold
            : fitness >= threshold;

    private static int RequiredReduction(
        int initialDistance,
        double pathFraction)
    {
        if (initialDistance <= 0)
        {
            return 0;
        }

        return Math.Max(
            1,
            (int)Math.Ceiling(initialDistance * pathFraction));
    }

    private void PromoteVisitedProbe(
        in TSolution solution,
        double fitness,
        long evaluationIndex,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner)
    {
        if (!context.WouldImprove(fitness))
        {
            return;
        }

        TSolution snapshot = solutionCloner.Clone(solution);
        context.PromoteOwnedExternalProbeSnapshot(snapshot, fitness, evaluationIndex);
    }

    private int GetValidatedDistance(
        in TSolution first,
        in TSolution second,
        IOptimizationProblem<TSolution> problem)
    {
        int distance = _distance.GetDistance(in first, in second, problem);

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

        TUndo undo = _moveOperator.CaptureUndo(in solution, in move);
        _moveOperator.Apply(ref solution, in move);

        try
        {
            double evaluatedFitness = problem.Evaluate(solution);

            if (double.IsNaN(evaluatedFitness))
            {
                throw new InvalidOperationException(
                    "The objective returned NaN during path relinking.");
            }

            return evaluatedFitness;
        }
        finally
        {
            _moveOperator.Undo(ref solution, in move, in undo);
        }
    }

    private static void ValidateCommon(
        double initiatingFitness,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps)
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
    }

    private static void ReturnBuffer(CandidateProbe[] buffer) =>
        ArrayPool<CandidateProbe>.Shared.Return(
            buffer,
            clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<CandidateProbe>());

    private readonly record struct CandidateProbe(
        TMove Move,
        double Fitness,
        long EvaluationIndex);

    private readonly record struct MoveSelectionResult(
        bool Selected,
        TMove Move,
        double Fitness,
        long EvaluationIndex,
        long CandidateEvaluations,
        StoppingDecision StoppingDecision);
}