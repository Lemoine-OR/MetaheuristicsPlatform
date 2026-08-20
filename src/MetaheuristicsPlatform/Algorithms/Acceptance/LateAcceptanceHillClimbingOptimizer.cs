using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>
/// High-performance final-form Late Acceptance Hill-Climbing optimizer.
/// Scientific basis: Burke &amp; Bykov (2017), DOI 10.1016/j.ejor.2016.07.012.
/// </summary>
public sealed class LateAcceptanceHillClimbingOptimizer<TSolution,TMove,TUndo> :
    IMetaheuristic<TSolution,LateAcceptanceParameters>
{
    private readonly IAcceptanceTrajectoryInitialSolutionGenerator<TSolution> _initial;
    private readonly IStochasticNeighborhood<TSolution,TMove> _neighborhood;
    private readonly IReversibleMoveOperator<TSolution,TMove,TUndo> _moveOperator;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution,TMove>? _delta;

    public LateAcceptanceHillClimbingOptimizer(
        IAcceptanceTrajectoryInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IStochasticNeighborhood<TSolution,TMove> neighborhood,
        IReversibleMoveOperator<TSolution,TMove,TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<TSolution,TMove>? deltaEvaluator = null)
    {
        _initial = initialSolutionGenerator ?? throw new ArgumentNullException(nameof(initialSolutionGenerator));
        _neighborhood = neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        _moveOperator = moveOperator ?? throw new ArgumentNullException(nameof(moveOperator));
        _delta = deltaEvaluator;
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "late-acceptance-hill-climbing-burke-bykov-2017",
        Name = "Late Acceptance Hill Climbing",
        Acronym = "LAHC",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces =
            SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = [ LateAcceptanceReferences.BurkeBykov2017 ]
    };

    public LateAcceptanceParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        LateAcceptanceParameters parameters,
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
            Descriptor, problem, solutionCloner, stoppingCriterion, options, callback, cancellationToken);

        TSolution solution = _initial.Create(problem, context.Random);
        var statistics = new TrajectoryStatisticsAccumulator();
        int failures = 0;

        var initialState = new LateAcceptanceState(
            double.NaN,
            problem.Sense.WorstValue(),
            double.NaN,
            parameters.HistoryLength,
            0,
            0,0,0,0,0,0,0,0);

        context.Start(initialState);
        double current = context.Evaluate(solution, initialState);

        var policy = new LateAcceptancePolicy(parameters.HistoryLength, current);
        var executor = new ReversibleTrajectoryStepExecutor<TSolution,TMove,TUndo>(
            _moveOperator,
            (in TSolution candidate) => problem.Evaluate(candidate),
            policy,
            _delta);

        LateAcceptanceState state = CreateState(
            current,
            context.State.BestFitness,
            policy,
            in statistics,
            failures);

        StoppingDecision stop = context.EvaluateStopping(state);
        if (stop.ShouldStop)
            return context.Complete(stop, state);

        long transition = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_neighborhood.TrySampleMove(in solution, context.Random, out TMove move))
            {
                failures++;
                state = CreateState(
                    current,
                    context.State.BestFitness,
                    policy,
                    in statistics,
                    failures);

                if (failures >= parameters.MaximumConsecutiveSamplingFailures)
                {
                    return context.Complete(
                        StoppingDecision.Stop(
                            "NeighborhoodExhausted",
                            "The stochastic neighborhood could not provide a move within the configured consecutive-failure limit."),
                        state);
                }

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);

                continue;
            }

            failures = 0;
            transition++;

            TrajectoryStepResult step = executor.Execute(
                ref solution,
                current,
                context.State.BestFitness,
                in move,
                transition,
                problem.Sense,
                context.Random,
                cancellationToken);

            TrajectoryStepEvaluationAccounting.RegisterVisitedStep(
                context,
                solutionCloner,
                in solution,
                in step);

            current = step.ResultingObjective;
            statistics.Record(in step);

            // Burke-Bykov final form: update only by strict history improvement,
            // but advance the circular history position after every sampled candidate.
            policy.CompleteTransition(problem.Sense, current);

            state = CreateState(
                current,
                context.State.BestFitness,
                policy,
                in statistics,
                failures);

            context.CompleteIteration(current, state);
            stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }
    }

    private static LateAcceptanceState CreateState(
        double current,
        double best,
        LateAcceptancePolicy policy,
        in TrajectoryStatisticsAccumulator statistics,
        int failures) =>
        new(
            current,
            best,
            policy.CurrentReference,
            policy.HistoryLength,
            policy.CurrentIndex,
            statistics.Attempts,
            statistics.Accepted,
            statistics.Improving,
            statistics.Equal,
            statistics.Worsening,
            statistics.DeltaEvaluations,
            statistics.FullEvaluations,
            failures);
}
