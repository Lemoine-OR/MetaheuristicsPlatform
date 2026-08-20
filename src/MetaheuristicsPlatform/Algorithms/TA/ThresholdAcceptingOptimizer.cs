using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>
/// Generic high-performance Dueck-Scheuer Threshold Accepting optimizer.
/// </summary>
/// <remarks>
/// The method reuses the generic reversible trajectory executor. With an exact move
/// objective evaluator, rejected moves are never applied and accepted moves are applied
/// exactly once. Acceptance itself is deterministic:
///
/// degradation(candidate,current) &lt;= threshold.
///
/// Scientific basis: Dueck &amp; Scheuer (1990),
/// DOI 10.1016/0021-9991(90)90201-B.
/// </remarks>
public sealed class ThresholdAcceptingOptimizer<
    TSolution,
    TMove,
    TUndo> :
    IMetaheuristic<
        TSolution,
        ThresholdAcceptingParameters>
{
    private readonly IThresholdAcceptingInitialSolutionGenerator<TSolution>
        _initialSolutionGenerator;

    private readonly IStochasticNeighborhood<
        TSolution,
        TMove> _neighborhood;

    private readonly IReversibleMoveOperator<
        TSolution,
        TMove,
        TUndo> _moveOperator;

    private readonly IMoveObjectiveDeltaEvaluator<
        TSolution,
        TMove>? _deltaEvaluator;

    public ThresholdAcceptingOptimizer(
        IThresholdAcceptingInitialSolutionGenerator<TSolution>
            initialSolutionGenerator,
        IStochasticNeighborhood<TSolution, TMove>
            neighborhood,
        IReversibleMoveOperator<
            TSolution,
            TMove,
            TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<
            TSolution,
            TMove>? deltaEvaluator = null)
    {
        _initialSolutionGenerator =
            initialSolutionGenerator ??
            throw new ArgumentNullException(
                nameof(initialSolutionGenerator));

        _neighborhood =
            neighborhood ??
            throw new ArgumentNullException(
                nameof(neighborhood));

        _moveOperator =
            moveOperator ??
            throw new ArgumentNullException(
                nameof(moveOperator));

        _deltaEvaluator =
            deltaEvaluator;
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id =
                "threshold-accepting-dueck-scheuer-1990",
            Name =
                "Threshold Accepting",
            Acronym =
                "TA",
            SolutionModel =
                MetaheuristicSolutionModel.SingleSolution,
            Families =
                MetaheuristicFamily.TrajectoryBased |
                MetaheuristicFamily.LocalSearch,
            Mechanisms =
                MetaheuristicMechanism.Neighborhood |
                MetaheuristicMechanism.Trajectory,
            SearchSpaces =
                SearchSpaceKind.Continuous |
                SearchSpaceKind.Binary |
                SearchSpaceKind.Integer |
                SearchSpaceKind.Permutation |
                SearchSpaceKind.Combinatorial |
                SearchSpaceKind.Mixed,
            IsStochastic =
                true,
            References =
            [
                ThresholdAcceptingReferences.DueckScheuer1990,
                ThresholdAcceptingReferences.WinkerFang1997
            ]
        };

    public ThresholdAcceptingParameters
        CreateDefaultParameters() =>
        new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        ThresholdAcceptingParameters parameters,
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

        TSolution solution =
            _initialSolutionGenerator.Create(
                problem,
                context.Random);

        double threshold =
            parameters.InitialThreshold;

        var acceptancePolicy =
            new ThresholdAcceptancePolicy(
                threshold);

        IThresholdAcceptingSchedule thresholdSchedule =
            parameters.CreateThresholdSchedule();

        var executor =
            new ReversibleTrajectoryStepExecutor<
                TSolution,
                TMove,
                TUndo>(
                _moveOperator,
                (
                    in TSolution candidate) =>
                    problem.Evaluate(
                        candidate),
                acceptancePolicy,
                _deltaEvaluator);

        var statistics =
            new TrajectoryStatisticsAccumulator();

        long thresholdLevel = 0;
        int transitionsInLevel = 0;
        int consecutiveSamplingFailures = 0;

        var initialState =
            new ThresholdAcceptingState(
                CurrentObjective:
                    double.NaN,
                BestObjective:
                    problem.Sense.WorstValue(),
                Threshold:
                    threshold,
                ThresholdLevel:
                    thresholdLevel,
                TransitionsInCurrentLevel:
                    transitionsInLevel,
                AttemptedTransitions:
                    0,
                AcceptedTransitions:
                    0,
                ImprovingTransitions:
                    0,
                EqualTransitions:
                    0,
                WorseningTransitions:
                    0,
                DeltaEvaluations:
                    0,
                FullEvaluations:
                    0,
                ConsecutiveSamplingFailures:
                    0);

        context.Start(
            initialState);

        double currentObjective =
            context.Evaluate(
                solution,
                initialState);

        ThresholdAcceptingState state =
            CreateState(
                currentObjective,
                context.State.BestFitness,
                threshold,
                thresholdLevel,
                transitionsInLevel,
                in statistics,
                consecutiveSamplingFailures);

        StoppingDecision stop =
            context.EvaluateStopping(
                state);

        if (stop.ShouldStop)
        {
            return
                context.Complete(
                    stop,
                    state);
        }

        if (parameters.StopAtMinimumThreshold &&
            threshold <= parameters.MinimumThreshold)
        {
            return
                context.Complete(
                    StoppingDecision.Stop(
                        "MinimumThreshold",
                        "The Threshold Accepting threshold is already at the configured minimum."),
                    state);
        }

        long transitionIndex = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_neighborhood.TrySampleMove(
                    in solution,
                    context.Random,
                    out TMove move))
            {
                consecutiveSamplingFailures++;

                state =
                    CreateState(
                        currentObjective,
                        context.State.BestFitness,
                        threshold,
                        thresholdLevel,
                        transitionsInLevel,
                        in statistics,
                        consecutiveSamplingFailures);

                if (consecutiveSamplingFailures >=
                    parameters.MaximumConsecutiveSamplingFailures)
                {
                    return
                        context.Complete(
                            StoppingDecision.Stop(
                                "NeighborhoodExhausted",
                                "The stochastic neighborhood could not provide a move within the configured consecutive-failure limit."),
                            state);
                }

                stop =
                    context.EvaluateStopping(
                        state);

                if (stop.ShouldStop)
                {
                    return
                        context.Complete(
                            stop,
                            state);
                }

                continue;
            }

            consecutiveSamplingFailures = 0;
            transitionIndex++;

            acceptancePolicy.SetThreshold(
                threshold);

            TrajectoryStepResult step =
                executor.Execute(
                    ref solution,
                    currentObjective,
                    context.State.BestFitness,
                    in move,
                    transitionIndex,
                    problem.Sense,
                    context.Random,
                    cancellationToken);

            TrajectoryStepEvaluationAccounting.RegisterVisitedStep(
                context,
                solutionCloner,
                in solution,
                in step);

            currentObjective =
                step.ResultingObjective;

            statistics.Record(
                in step);

            transitionsInLevel++;

            if (transitionsInLevel >=
                parameters.TransitionsPerThresholdLevel)
            {
                thresholdLevel++;

                var scheduleContext =
                    new ThresholdAcceptingScheduleContext(
                        CompletedThresholdLevels:
                            thresholdLevel,
                        AttemptedTransitions:
                            statistics.Attempts,
                        AcceptedTransitions:
                            statistics.Accepted,
                        InitialThreshold:
                            parameters.InitialThreshold,
                        CurrentThreshold:
                            threshold);

                double nextThreshold =
                    thresholdSchedule.GetNextThreshold(
                        in scheduleContext);

                if (!double.IsFinite(nextThreshold) ||
                    nextThreshold < 0.0)
                {
                    throw new InvalidOperationException(
                        "The Threshold Accepting schedule returned a non-finite or negative threshold.");
                }

                if (nextThreshold > threshold)
                {
                    throw new InvalidOperationException(
                        "The v0.33 monotone Threshold Accepting schedule returned a threshold larger than the current threshold. Non-monotone threshold control is reserved for dedicated acceptance methods.");
                }

                threshold =
                    Math.Max(
                        nextThreshold,
                        parameters.MinimumThreshold);

                transitionsInLevel = 0;
            }

            state =
                CreateState(
                    currentObjective,
                    context.State.BestFitness,
                    threshold,
                    thresholdLevel,
                    transitionsInLevel,
                    in statistics,
                    consecutiveSamplingFailures);

            context.CompleteIteration(
                currentObjective,
                state);

            if (parameters.StopAtMinimumThreshold &&
                threshold <=
                    parameters.MinimumThreshold)
            {
                return
                    context.Complete(
                        StoppingDecision.Stop(
                            "MinimumThreshold",
                            "The Threshold Accepting threshold reached the configured minimum."),
                        state);
            }

            stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return
                    context.Complete(
                        stop,
                        state);
            }
        }
    }

    private static ThresholdAcceptingState CreateState(
        double currentObjective,
        double bestObjective,
        double threshold,
        long thresholdLevel,
        int transitionsInLevel,
        in TrajectoryStatisticsAccumulator statistics,
        int consecutiveSamplingFailures) =>
        new(
            currentObjective,
            bestObjective,
            threshold,
            thresholdLevel,
            transitionsInLevel,
            statistics.Attempts,
            statistics.Accepted,
            statistics.Improving,
            statistics.Equal,
            statistics.Worsening,
            statistics.DeltaEvaluations,
            statistics.FullEvaluations,
            consecutiveSamplingFailures);
}