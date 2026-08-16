using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Canonical Greedy Randomized Adaptive Search Procedure (GRASP) of Feo and Resende.
/// Each outer iteration constructs one randomized greedy solution, then improves it by
/// a reusable local-search procedure. Best-so-far ownership remains in OptimizationContext.
/// </summary>
public sealed class GraspOptimizer<TSolution> :
    IMetaheuristic<TSolution, GraspParameters>
{
    private readonly IGraspConstructionProcedure<TSolution> _construction;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;

    /// <summary>Creates a canonical GRASP composition.</summary>
    public GraspOptimizer(
        IGraspConstructionProcedure<TSolution> construction,
        ILocalSearchProcedure<TSolution> localSearch)
    {
        _construction =
            construction ?? throw new ArgumentNullException(nameof(construction));
        _localSearch =
            localSearch ?? throw new ArgumentNullException(nameof(localSearch));
    }

    /// <inheritdoc />
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "grasp-feo-resende-1995",
        Name = "GRASP - Feo-Resende",
        Acronym = "GRASP",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families =
            MetaheuristicFamily.Constructive |
            MetaheuristicFamily.LocalSearch,
        Mechanisms =
            MetaheuristicMechanism.Constructive |
            MetaheuristicMechanism.Neighborhood,
        SearchSpaces =
            SearchSpaceKind.Binary |
            SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial |
            SearchSpaceKind.Mixed |
            SearchSpaceKind.Continuous,
        IsStochastic = true,
        References =
        [
            GraspReferences.FeoResende1989,
            GraspReferences.FeoResende1995
        ]
    };

    /// <inheritdoc />
    public GraspParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        GraspParameters parameters,
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

        var state = new GraspState(
            OuterIterationsCompleted: 0,
            ConstructionSteps: 0,
            GreedyScoreEvaluations: 0,
            AcceptedLocalMoves: 0,
            Alpha: parameters.Alpha);

        context.Start(state);

        long totalConstructionSteps = 0;
        long totalGreedyScoreEvaluations = 0;
        long totalAcceptedLocalMoves = 0;

        for (int outerIteration = 1;
             outerIteration <= parameters.MaximumIterations;
             outerIteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GraspConstructionResult<TSolution> constructionResult =
                _construction.Construct(
                    problem,
                    context.Random,
                    parameters.Alpha,
                    parameters.MaximumConstructionSteps,
                    cancellationToken);

            totalConstructionSteps +=
                constructionResult.ConstructionSteps;
            totalGreedyScoreEvaluations +=
                constructionResult.GreedyScoreEvaluations;

            state = new GraspState(
                outerIteration - 1,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                parameters.Alpha);

            TSolution solution =
                constructionResult.Solution;

            double fitness =
                context.Evaluate(
                    solution,
                    state);

            StoppingDecision stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }

            LocalSearchProcedureResult localResult =
                _localSearch.Improve(
                    ref solution,
                    fitness,
                    context,
                    solutionCloner,
                    cancellationToken);

            totalAcceptedLocalMoves +=
                localResult.AcceptedMoves;

            state = new GraspState(
                outerIteration,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                parameters.Alpha);

            if (localResult.StoppingDecision.ShouldStop)
            {
                return context.Complete(
                    localResult.StoppingDecision,
                    state);
            }

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumGraspIterations",
                "The configured GRASP outer-iteration limit was reached."),
            new GraspState(
                parameters.MaximumIterations,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                parameters.Alpha));
    }
}

/// <summary>Observable GRASP state for callbacks and custom stopping criteria.</summary>
public readonly record struct GraspState(
    int OuterIterationsCompleted,
    long ConstructionSteps,
    long GreedyScoreEvaluations,
    long AcceptedLocalMoves,
    double Alpha);
