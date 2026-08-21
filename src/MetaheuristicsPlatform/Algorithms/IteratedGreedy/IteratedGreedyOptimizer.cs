using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

/// <summary>
/// Generic Iterated Greedy optimizer following the destruction-reconstruction framework of
/// Ruiz and Stützle, with optional reusable local improvement and pluggable acceptance.
/// v0.38.0 adds optional destruction-size control and partial-solution improvement without
/// changing the canonical stable algorithm identity.
/// </summary>
public sealed class IteratedGreedyOptimizer<TSolution,TRemoved> :
    IMetaheuristic<TSolution,IteratedGreedyParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initial;
    private readonly IIteratedGreedyDestruction<TSolution,TRemoved> _destruction;
    private readonly IIteratedGreedyConstruction<TSolution,TRemoved> _construction;
    private readonly IIteratedGreedyAcceptancePolicy _acceptance;
    private readonly IIteratedGreedyDestructionSizePolicy _destructionSizePolicy;
    private readonly IIteratedGreedyPartialSolutionImprovement<TSolution,TRemoved>? _partialSolutionImprovement;
    private readonly ILocalSearchProcedure<TSolution>? _localSearch;

    public IteratedGreedyOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IIteratedGreedyDestruction<TSolution,TRemoved> destruction,
        IIteratedGreedyConstruction<TSolution,TRemoved> construction,
        IIteratedGreedyAcceptancePolicy acceptance,
        ILocalSearchProcedure<TSolution>? localSearch = null)
        : this(
            initialSolutionGenerator,
            destruction,
            construction,
            acceptance,
            FixedIteratedGreedyDestructionSizePolicy.Instance,
            partialSolutionImprovement: null,
            localSearch: localSearch)
    {
    }

    /// <summary>
    /// Advanced composition constructor. The original constructor remains source compatible.
    /// </summary>
    public IteratedGreedyOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IIteratedGreedyDestruction<TSolution,TRemoved> destruction,
        IIteratedGreedyConstruction<TSolution,TRemoved> construction,
        IIteratedGreedyAcceptancePolicy acceptance,
        IIteratedGreedyDestructionSizePolicy destructionSizePolicy,
        IIteratedGreedyPartialSolutionImprovement<TSolution,TRemoved>? partialSolutionImprovement = null,
        ILocalSearchProcedure<TSolution>? localSearch = null)
    {
        _initial =
            initialSolutionGenerator ??
            throw new ArgumentNullException(nameof(initialSolutionGenerator));

        _destruction =
            destruction ??
            throw new ArgumentNullException(nameof(destruction));

        _construction =
            construction ??
            throw new ArgumentNullException(nameof(construction));

        _acceptance =
            acceptance ??
            throw new ArgumentNullException(nameof(acceptance));

        _destructionSizePolicy =
            destructionSizePolicy ??
            throw new ArgumentNullException(nameof(destructionSizePolicy));

        _partialSolutionImprovement =
            partialSolutionImprovement;

        _localSearch =
            localSearch;
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "iterated-greedy-ruiz-stutzle-2007",
        Name = "Iterated Greedy - Ruiz-Stützle",
        Acronym = "IG",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.Constructive,
        Mechanisms = MetaheuristicMechanism.Trajectory | MetaheuristicMechanism.Constructive,
        SearchSpaces =
            SearchSpaceKind.Binary | SearchSpaceKind.Integer | SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References =
        [
            IteratedGreedyReferences.RuizStutzle2007,
            IteratedGreedyReferences.StutzleRuiz2025
        ]
    };

    public IteratedGreedyParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        IteratedGreedyParameters parameters,
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

        long acceptedCandidates = 0;
        long rejectedCandidates = 0;
        long localSearchInvocations = 0;
        long acceptedLocalSearchMoves = 0;
        int consecutiveNonImprovingIterations = 0;

        var initialState = new IteratedGreedyState(
            0,
            double.NaN,
            problem.Sense.WorstValue(),
            double.NaN,
            parameters.DestructionSize,
            0,0,0,0);

        context.Start(initialState);

        TSolution current =
            _initial.Create(
                problem,
                context.Random);

        double currentObjective =
            context.Evaluate(
                current,
                initialState);

        if (_localSearch is not null)
        {
            LocalSearchProcedureResult initialImprovement =
                _localSearch.Improve(
                    ref current,
                    currentObjective,
                    context,
                    solutionCloner,
                    cancellationToken);

            localSearchInvocations++;
            acceptedLocalSearchMoves +=
                initialImprovement.AcceptedMoves;

            currentObjective =
                initialImprovement.Fitness;

            if (initialImprovement.StoppingDecision.ShouldStop)
            {
                return context.Complete(
                    initialImprovement.StoppingDecision,
                    CreateState(
                        0,
                        currentObjective,
                        context.State.BestFitness,
                        currentObjective,
                        parameters.DestructionSize,
                        acceptedCandidates,
                        rejectedCandidates,
                        localSearchInvocations,
                        acceptedLocalSearchMoves));
            }
        }

        IteratedGreedyState state =
            CreateState(
                0,
                currentObjective,
                context.State.BestFitness,
                currentObjective,
                parameters.DestructionSize,
                acceptedCandidates,
                rejectedCandidates,
                localSearchInvocations,
                acceptedLocalSearchMoves);

        StoppingDecision stop =
            context.EvaluateStopping(state);

        if (stop.ShouldStop)
            return context.Complete(stop, state);

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long improvementCountBeforeIteration =
                context.State.ImprovementCount;

            var sizeContext =
                new IteratedGreedyDestructionSizeContext(
                    problem.Sense,
                    iteration,
                    parameters.DestructionSize,
                    consecutiveNonImprovingIterations,
                    currentObjective,
                    context.State.BestFitness);

            int destructionSize =
                _destructionSizePolicy.SelectDestructionSize(
                    in sizeContext);

            if (destructionSize <= 0)
            {
                throw new InvalidOperationException(
                    "The Iterated Greedy destruction-size policy returned a non-positive size.");
            }

            TSolution candidate =
                solutionCloner.Clone(current);

            TRemoved removed =
                _destruction.Destroy(
                    ref candidate,
                    destructionSize,
                    problem,
                    context.Random);

            _partialSolutionImprovement?.Improve(
                ref candidate,
                in removed,
                problem,
                context.Random,
                cancellationToken);

            _construction.Reconstruct(
                ref candidate,
                in removed,
                problem,
                context.Random);

            state =
                CreateState(
                    iteration - 1,
                    currentObjective,
                    context.State.BestFitness,
                    double.NaN,
                    destructionSize,
                    acceptedCandidates,
                    rejectedCandidates,
                    localSearchInvocations,
                    acceptedLocalSearchMoves);

            double candidateObjective =
                context.Evaluate(
                    candidate,
                    state);

            // Audit fix v0.38.0: stopping criteria now receive the objective of the
            // complete reconstructed candidate that has just been evaluated.
            state =
                CreateState(
                    iteration - 1,
                    currentObjective,
                    context.State.BestFitness,
                    candidateObjective,
                    destructionSize,
                    acceptedCandidates,
                    rejectedCandidates,
                    localSearchInvocations,
                    acceptedLocalSearchMoves);

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);

            if (_localSearch is not null)
            {
                LocalSearchProcedureResult localResult =
                    _localSearch.Improve(
                        ref candidate,
                        candidateObjective,
                        context,
                        solutionCloner,
                        cancellationToken);

                localSearchInvocations++;
                acceptedLocalSearchMoves +=
                    localResult.AcceptedMoves;

                candidateObjective =
                    localResult.Fitness;

                if (localResult.StoppingDecision.ShouldStop)
                {
                    return context.Complete(
                        localResult.StoppingDecision,
                        CreateState(
                            iteration - 1,
                            currentObjective,
                            context.State.BestFitness,
                            candidateObjective,
                            destructionSize,
                            acceptedCandidates,
                            rejectedCandidates,
                            localSearchInvocations,
                            acceptedLocalSearchMoves));
                }
            }

            var acceptanceContext =
                new IteratedGreedyAcceptanceContext(
                    problem.Sense,
                    iteration,
                    currentObjective,
                    candidateObjective,
                    context.State.BestFitness);

            if (_acceptance.ShouldAccept(
                    in acceptanceContext,
                    context.Random))
            {
                current =
                    candidate;

                currentObjective =
                    candidateObjective;

                acceptedCandidates++;
            }
            else
            {
                rejectedCandidates++;
            }

            if (context.State.ImprovementCount >
                improvementCountBeforeIteration)
            {
                consecutiveNonImprovingIterations = 0;
            }
            else
            {
                consecutiveNonImprovingIterations =
                    checked(
                        consecutiveNonImprovingIterations +
                        1);
            }

            state =
                CreateState(
                    iteration,
                    currentObjective,
                    context.State.BestFitness,
                    candidateObjective,
                    destructionSize,
                    acceptedCandidates,
                    rejectedCandidates,
                    localSearchInvocations,
                    acceptedLocalSearchMoves);

            context.CompleteIteration(
                currentObjective,
                state);

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        state =
            CreateState(
                parameters.MaximumIterations,
                currentObjective,
                context.State.BestFitness,
                currentObjective,
                parameters.DestructionSize,
                acceptedCandidates,
                rejectedCandidates,
                localSearchInvocations,
                acceptedLocalSearchMoves);

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumIteratedGreedyIterations",
                "The configured Iterated Greedy cycle limit was reached."),
            state);
    }

    private static IteratedGreedyState CreateState(
        int iterationsCompleted,
        double currentObjective,
        double bestObjective,
        double lastCandidateObjective,
        int destructionSize,
        long acceptedCandidates,
        long rejectedCandidates,
        long localSearchInvocations,
        long acceptedLocalSearchMoves) =>
        new(
            iterationsCompleted,
            currentObjective,
            bestObjective,
            lastCandidateObjective,
            destructionSize,
            acceptedCandidates,
            rejectedCandidates,
            localSearchInvocations,
            acceptedLocalSearchMoves);
}
