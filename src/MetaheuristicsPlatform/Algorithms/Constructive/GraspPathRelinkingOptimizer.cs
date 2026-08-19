using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// GRASP with quality/diversity elite memory, configurable pairwise path relinking,
/// and optional generational evolutionary path-relinking post-optimization.
/// </summary>
public sealed class GraspPathRelinkingOptimizer<TSolution> :
    IMetaheuristic<TSolution, GraspPathRelinkingParameters>
{
    private readonly IGraspConstructionProcedure<TSolution> _construction;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;
    private readonly IPathRelinkingProcedure<TSolution> _pathRelinking;
    private readonly IPathRelinkingDistance<TSolution> _distance;

    public GraspPathRelinkingOptimizer(
        IGraspConstructionProcedure<TSolution> construction,
        ILocalSearchProcedure<TSolution> localSearch,
        IPathRelinkingProcedure<TSolution> pathRelinking,
        IPathRelinkingDistance<TSolution> distance)
    {
        _construction =
            construction ?? throw new ArgumentNullException(nameof(construction));
        _localSearch =
            localSearch ?? throw new ArgumentNullException(nameof(localSearch));
        _pathRelinking =
            pathRelinking ?? throw new ArgumentNullException(nameof(pathRelinking));
        _distance =
            distance ?? throw new ArgumentNullException(nameof(distance));
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "grasp-path-relinking",
        Name = "GRASP with Path Relinking",
        Acronym = "GRASP-PR",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families =
            MetaheuristicFamily.Constructive |
            MetaheuristicFamily.LocalSearch |
            MetaheuristicFamily.Hybrid,
        Mechanisms =
            MetaheuristicMechanism.Constructive |
            MetaheuristicMechanism.Neighborhood |
            MetaheuristicMechanism.MemoryBased |
            MetaheuristicMechanism.Hybrid,
        SearchSpaces =
            SearchSpaceKind.Binary |
            SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial |
            SearchSpaceKind.Mixed,
        IsStochastic = true,
        References =
        [
            GraspReferences.FeoResende1995,
            GraspReferences.ResendeRibeiro2003,
            GraspReferences.ResendeWerneck2004,
            GraspReferences.AiexResendePardalosToraldo2005,
            GraspReferences.ResendeMartiGallegoDuarte2010,
            GraspReferences.RibeiroResende2012
        ]
    };

    public GraspPathRelinkingParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        GraspPathRelinkingParameters parameters,
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

        PathRelinkingExecutionOptions pathRelinkingOptions =
            parameters.CreatePathRelinkingExecutionOptions();

        PathRelinkingExecutionOptions evolutionaryOptions =
            parameters.CreateEvolutionaryPathRelinkingExecutionOptions();

        if ((!pathRelinkingOptions.IsCanonicalGreedyForward ||
             (parameters.EvolutionaryPathRelinkingEnabled &&
              !evolutionaryOptions.IsCanonicalGreedyForward)) &&
            _pathRelinking is not IAdvancedPathRelinkingProcedure<TSolution>)
        {
            throw new InvalidOperationException(
                "Advanced path-relinking parameters require an " +
                "IAdvancedPathRelinkingProcedure<TSolution> implementation.");
        }

        var context = new OptimizationContext<TSolution>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        var elitePool = new EliteSolutionPool<TSolution>(
            parameters.ElitePoolSize,
            parameters.MinimumEliteDistance,
            _distance,
            problem,
            solutionCloner);

        var state = CreateState(
            outerIterationsCompleted: 0,
            constructionSteps: 0,
            greedyScoreEvaluations: 0,
            acceptedLocalMoves: 0,
            pathRelinkingInvocations: 0,
            pathSteps: 0,
            pathCandidateEvaluations: 0,
            elitePoolCount: 0,
            elitePoolUpdates: 0);

        context.Start(state);

        long totalConstructionSteps = 0;
        long totalGreedyScoreEvaluations = 0;
        long totalAcceptedLocalMoves = 0;
        long pathRelinkingInvocations = 0;
        long totalPathSteps = 0;
        long totalPathCandidateEvaluations = 0;
        long elitePoolUpdates = 0;

        int evolutionaryGenerationsCompleted = 0;
        long evolutionaryPairRelinkings = 0;
        long evolutionaryPathSteps = 0;
        long evolutionaryCandidateEvaluations = 0;
        long evolutionaryAcceptedLocalMoves = 0;
        long evolutionaryElitePoolUpdates = 0;

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

            TSolution solution =
                constructionResult.Solution;

            state = CreateState(
                outerIteration - 1,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                pathRelinkingInvocations,
                totalPathSteps,
                totalPathCandidateEvaluations,
                elitePool.Count,
                elitePoolUpdates);

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

            fitness =
                localResult.Fitness;
            totalAcceptedLocalMoves +=
                localResult.AcceptedMoves;

            if (localResult.StoppingDecision.ShouldStop)
            {
                state = CreateState(
                    outerIteration - 1,
                    totalConstructionSteps,
                    totalGreedyScoreEvaluations,
                    totalAcceptedLocalMoves,
                    pathRelinkingInvocations,
                    totalPathSteps,
                    totalPathCandidateEvaluations,
                    elitePool.Count,
                    elitePoolUpdates);

                return context.Complete(
                    localResult.StoppingDecision,
                    state);
            }

            if (elitePool.TrySelectGuide(
                    in solution,
                    context.Random,
                    out TSolution guidingSolution,
                    out double guidingFitness))
            {
                pathRelinkingInvocations++;

                PathRelinkingProcedureResult<TSolution> relinkingResult;

                if (_pathRelinking is
                    IAdvancedPathRelinkingProcedure<TSolution> advancedPathRelinking)
                {
                    relinkingResult =
                        advancedPathRelinking.RelinkAdvanced(
                            in solution,
                            fitness,
                            in guidingSolution,
                            guidingFitness,
                            pathRelinkingOptions,
                            context,
                            solutionCloner,
                            parameters.MaximumPathSteps,
                            cancellationToken);
                }
                else
                {
                    relinkingResult =
                        _pathRelinking.Relink(
                            in solution,
                            fitness,
                            in guidingSolution,
                            context,
                            solutionCloner,
                            parameters.MaximumPathSteps,
                            cancellationToken);
                }

                totalPathSteps +=
                    relinkingResult.PathSteps;
                totalPathCandidateEvaluations +=
                    relinkingResult.CandidateEvaluations;

                if (relinkingResult.StoppingDecision.ShouldStop)
                {
                    state = CreateState(
                        outerIteration - 1,
                        totalConstructionSteps,
                        totalGreedyScoreEvaluations,
                        totalAcceptedLocalMoves,
                        pathRelinkingInvocations,
                        totalPathSteps,
                        totalPathCandidateEvaluations,
                        elitePool.Count,
                        elitePoolUpdates);

                    return context.Complete(
                        relinkingResult.StoppingDecision,
                        state);
                }

                solution =
                    relinkingResult.BestSolution;
                fitness =
                    relinkingResult.BestFitness;
            }

            if (elitePool.TryAdd(
                    in solution,
                    fitness,
                    out _))
            {
                elitePoolUpdates++;
            }

            state = CreateState(
                outerIteration,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                pathRelinkingInvocations,
                totalPathSteps,
                totalPathCandidateEvaluations,
                elitePool.Count,
                elitePoolUpdates);

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

        if (parameters.EvolutionaryPathRelinkingEnabled &&
            elitePool.Count >= 2)
        {
            var evolutionaryProcedure =
                new EvolutionaryPathRelinkingProcedure<TSolution>(
                    _pathRelinking,
                    _localSearch);

            EvolutionaryPathRelinkingResult<TSolution> evolutionaryResult =
                evolutionaryProcedure.Evolve(
                    elitePool,
                    evolutionaryOptions,
                    context,
                    solutionCloner,
                    parameters.MaximumEvolutionaryGenerations,
                    parameters.MaximumEvolutionaryPathSteps,
                    parameters.ImproveEvolutionaryOffspring,
                    cancellationToken);

            evolutionaryGenerationsCompleted =
                evolutionaryResult.GenerationsCompleted;
            evolutionaryPairRelinkings =
                evolutionaryResult.PairRelinkings;
            evolutionaryPathSteps =
                evolutionaryResult.PathSteps;
            evolutionaryCandidateEvaluations =
                evolutionaryResult.CandidateEvaluations;
            evolutionaryAcceptedLocalMoves =
                evolutionaryResult.AcceptedLocalMoves;
            evolutionaryElitePoolUpdates =
                evolutionaryResult.ElitePoolUpdates;

            pathRelinkingInvocations +=
                evolutionaryPairRelinkings;
            totalPathSteps +=
                evolutionaryPathSteps;
            totalPathCandidateEvaluations +=
                evolutionaryCandidateEvaluations;
            totalAcceptedLocalMoves +=
                evolutionaryAcceptedLocalMoves;
            elitePoolUpdates +=
                evolutionaryElitePoolUpdates;

            state = CreateState(
                parameters.MaximumIterations,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                pathRelinkingInvocations,
                totalPathSteps,
                totalPathCandidateEvaluations,
                elitePool.Count,
                elitePoolUpdates,
                evolutionaryGenerationsCompleted,
                evolutionaryPairRelinkings,
                evolutionaryPathSteps,
                evolutionaryCandidateEvaluations,
                evolutionaryAcceptedLocalMoves,
                evolutionaryElitePoolUpdates);

            if (evolutionaryResult.StoppingDecision.ShouldStop)
            {
                return context.Complete(
                    evolutionaryResult.StoppingDecision,
                    state);
            }
        }

        string completionMessage =
            parameters.EvolutionaryPathRelinkingEnabled
                ? "The configured GRASP Path Relinking outer-iteration limit was reached and the enabled evolutionary path-relinking post-optimization phase completed."
                : "The configured GRASP Path Relinking outer-iteration limit was reached.";

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumGraspPathRelinkingIterations",
                completionMessage),
            CreateState(
                parameters.MaximumIterations,
                totalConstructionSteps,
                totalGreedyScoreEvaluations,
                totalAcceptedLocalMoves,
                pathRelinkingInvocations,
                totalPathSteps,
                totalPathCandidateEvaluations,
                elitePool.Count,
                elitePoolUpdates,
                evolutionaryGenerationsCompleted,
                evolutionaryPairRelinkings,
                evolutionaryPathSteps,
                evolutionaryCandidateEvaluations,
                evolutionaryAcceptedLocalMoves,
                evolutionaryElitePoolUpdates));
    }

    private static GraspPathRelinkingState CreateState(
        int outerIterationsCompleted,
        long constructionSteps,
        long greedyScoreEvaluations,
        long acceptedLocalMoves,
        long pathRelinkingInvocations,
        long pathSteps,
        long pathCandidateEvaluations,
        int elitePoolCount,
        long elitePoolUpdates,
        int evolutionaryGenerationsCompleted = 0,
        long evolutionaryPairRelinkings = 0,
        long evolutionaryPathSteps = 0,
        long evolutionaryCandidateEvaluations = 0,
        long evolutionaryAcceptedLocalMoves = 0,
        long evolutionaryElitePoolUpdates = 0) =>
        new(
            outerIterationsCompleted,
            constructionSteps,
            greedyScoreEvaluations,
            acceptedLocalMoves,
            pathRelinkingInvocations,
            pathSteps,
            pathCandidateEvaluations,
            elitePoolCount,
            elitePoolUpdates)
        {
            EvolutionaryGenerationsCompleted =
                evolutionaryGenerationsCompleted,
            EvolutionaryPairRelinkings =
                evolutionaryPairRelinkings,
            EvolutionaryPathSteps =
                evolutionaryPathSteps,
            EvolutionaryCandidateEvaluations =
                evolutionaryCandidateEvaluations,
            EvolutionaryAcceptedLocalMoves =
                evolutionaryAcceptedLocalMoves,
            EvolutionaryElitePoolUpdates =
                evolutionaryElitePoolUpdates
        };
}

/// <summary>Observable state for GRASP with Path Relinking.</summary>
public readonly record struct GraspPathRelinkingState(
    int OuterIterationsCompleted,
    long ConstructionSteps,
    long GreedyScoreEvaluations,
    long AcceptedLocalMoves,
    long PathRelinkingInvocations,
    long PathSteps,
    long PathCandidateEvaluations,
    int ElitePoolCount,
    long ElitePoolUpdates)
{
    public int EvolutionaryGenerationsCompleted { get; init; }

    public long EvolutionaryPairRelinkings { get; init; }

    public long EvolutionaryPathSteps { get; init; }

    public long EvolutionaryCandidateEvaluations { get; init; }

    public long EvolutionaryAcceptedLocalMoves { get; init; }

    public long EvolutionaryElitePoolUpdates { get; init; }
}