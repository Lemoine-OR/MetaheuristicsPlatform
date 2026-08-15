using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

public abstract class LocalSearchOptimizerBase<
    TSolution,
    TMove,
    TUndo,
    TEnumerator> : IMetaheuristic<TSolution, LocalSearchParameters>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> _neighborhood;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo> _moveOperator;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>? _deltaEvaluator;
    private readonly IMoveApplicability<TSolution, TMove>? _moveApplicability;
    private readonly LocalSearchSelectionPolicy _selectionPolicy;

    protected LocalSearchOptimizerBase(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        LocalSearchSelectionPolicy selectionPolicy,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null,
        IMoveApplicability<TSolution, TMove>? moveApplicability = null)
    {
        _initialGenerator = initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _neighborhood = neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        _moveOperator = moveOperator ?? throw new ArgumentNullException(nameof(moveOperator));
        _selectionPolicy = selectionPolicy;
        _deltaEvaluator = deltaEvaluator;
        _moveApplicability = moveApplicability;
    }

    public abstract MetaheuristicDescriptor Descriptor { get; }
    public LocalSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        LocalSearchParameters parameters,
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
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        context.Start();
        TSolution solution = _initialGenerator.Create(problem, context.Random);
        double fitness = context.Evaluate(solution);

        StoppingDecision stop = context.EvaluateStopping();
        if (stop.ShouldStop)
            return context.Complete(stop);

        var procedure = new MoveLocalSearchProcedure<TSolution, TMove, TUndo, TEnumerator>(
            _neighborhood,
            _moveOperator,
            _selectionPolicy,
            _deltaEvaluator,
            _moveApplicability,
            parameters.MaximumAcceptedMoves);

        LocalSearchProcedureResult result = procedure.Improve(
            ref solution,
            fitness,
            context,
            solutionCloner,
            cancellationToken);

        if (result.StoppingDecision.ShouldStop)
            return context.Complete(result.StoppingDecision);

        string criterion = result.IsLocalOptimum ? "LocalOptimum" : "MaximumAcceptedMoves";
        string message = result.IsLocalOptimum
            ? "No strictly improving move remains in the configured neighborhood."
            : "The configured accepted-move limit was reached.";
        return context.Complete(StoppingDecision.Stop(criterion, message));
    }
}

/// <summary>Steepest-descent / best-improvement local search.</summary>
public sealed class BestImprovementLocalSearchOptimizer<
    TSolution,
    TMove,
    TUndo,
    TEnumerator> : LocalSearchOptimizerBase<TSolution, TMove, TUndo, TEnumerator>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    public BestImprovementLocalSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null,
        IMoveApplicability<TSolution, TMove>? moveApplicability = null)
        : base(initialGenerator, neighborhood, moveOperator,
            LocalSearchSelectionPolicy.BestImprovement,
            deltaEvaluator, moveApplicability) { }

    public override MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "local-search-best-improvement",
        Name = "Local Search - Best Improvement",
        Acronym = "LS-BI",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = false,
        References = new[] { NeighborhoodSearchReferences.Talbi2009 }
    };
}

/// <summary>First-descent / first-improvement local search.</summary>
public sealed class FirstImprovementLocalSearchOptimizer<
    TSolution,
    TMove,
    TUndo,
    TEnumerator> : LocalSearchOptimizerBase<TSolution, TMove, TUndo, TEnumerator>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    public FirstImprovementLocalSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null,
        IMoveApplicability<TSolution, TMove>? moveApplicability = null)
        : base(initialGenerator, neighborhood, moveOperator,
            LocalSearchSelectionPolicy.FirstImprovement,
            deltaEvaluator, moveApplicability) { }

    public override MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "local-search-first-improvement",
        Name = "Local Search - First Improvement",
        Acronym = "LS-FI",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = false,
        References = new[] { NeighborhoodSearchReferences.Talbi2009 }
    };
}
