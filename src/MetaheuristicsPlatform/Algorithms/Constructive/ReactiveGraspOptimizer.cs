using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Reactive GRASP of Prais and Ribeiro.
/// The RCL parameter alpha is sampled from a discrete probability distribution whose
/// weights are periodically updated from the average locally improved solution quality.
/// </summary>
public sealed class ReactiveGraspOptimizer<TSolution> :
    IMetaheuristic<TSolution, ReactiveGraspParameters>
{
    private readonly IGraspConstructionProcedure<TSolution> _construction;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;

    /// <summary>Creates a Reactive GRASP composition.</summary>
    public ReactiveGraspOptimizer(
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
        Id = "reactive-grasp-prais-ribeiro-2000",
        Name = "Reactive GRASP - Prais-Ribeiro",
        Acronym = "R-GRASP",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families =
            MetaheuristicFamily.Constructive |
            MetaheuristicFamily.LocalSearch,
        Mechanisms =
            MetaheuristicMechanism.Constructive |
            MetaheuristicMechanism.Neighborhood |
            MetaheuristicMechanism.Adaptive,
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
            GraspReferences.FeoResende1995,
            GraspReferences.PraisRibeiro2000
        ]
    };

    /// <inheritdoc />
    public ReactiveGraspParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        ReactiveGraspParameters parameters,
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

        var controller =
            new PraisRibeiroReactiveAlphaController(
                parameters.AlphaValues,
                parameters.ProbabilityUpdatePeriod,
                problem.Sense);

        var state = new ReactiveGraspState(
            OuterIterationsCompleted: 0,
            ConstructionSteps: 0,
            GreedyScoreEvaluations: 0,
            AcceptedLocalMoves: 0,
            CurrentAlpha: double.NaN,
            DistinctAlphaValuesObserved: 0,
            ProbabilityUpdates: 0);

        context.Start(state);

        long totalConstructionSteps = 0;
        long totalGreedyScoreEvaluations = 0;
        long totalAcceptedLocalMoves = 0;

        for (int outerIteration = 1;
             outerIteration <= parameters.MaximumIterations;
             outerIteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int alphaIndex =
                controller.SelectAlphaIndex(context.Random);

            double alpha =
                controller.GetAlpha(alphaIndex);

            GraspConstructionResult<TSolution> constructionResult =
                _construction.Construct(
                    problem,
                    context.Random,
                    alpha,
                    parameters.MaximumConstructionSteps,
                    cancellationToken);

            totalConstructionSteps +=
                constructionResult.ConstructionSteps;
            totalGreedyScoreEvaluations +=
                constructionResult.GreedyScoreEvaluations;

            state = new ReactiveGraspState(
                outerIteration - 1,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                alpha,
                controller.DistinctObserved,
                controller.ProbabilityUpdates);

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

            fitness = localResult.Fitness;
            totalAcceptedLocalMoves +=
                localResult.AcceptedMoves;

            if (localResult.StoppingDecision.ShouldStop)
            {
                state = new ReactiveGraspState(
                    outerIteration - 1,
                    totalConstructionSteps,
                    totalGreedyScoreEvaluations,
                    totalAcceptedLocalMoves,
                    alpha,
                    controller.DistinctObserved,
                    controller.ProbabilityUpdates);

                return context.Complete(
                    localResult.StoppingDecision,
                    state);
            }

            controller.Observe(
                alphaIndex,
                fitness);

            state = new ReactiveGraspState(
                outerIteration,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                alpha,
                controller.DistinctObserved,
                controller.ProbabilityUpdates);

            context.CompleteIteration(
                fitness,
                state);

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumReactiveGraspIterations",
                "The configured Reactive GRASP outer-iteration limit was reached."),
            new ReactiveGraspState(
                parameters.MaximumIterations,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                state.CurrentAlpha,
                controller.DistinctObserved,
                controller.ProbabilityUpdates));
    }
}

/// <summary>Observable Reactive GRASP state for callbacks and custom stopping criteria.</summary>
public readonly record struct ReactiveGraspState(
    int OuterIterationsCompleted,
    long ConstructionSteps,
    long GreedyScoreEvaluations,
    long AcceptedLocalMoves,
    double CurrentAlpha,
    int DistinctAlphaValuesObserved,
    int ProbabilityUpdates);
