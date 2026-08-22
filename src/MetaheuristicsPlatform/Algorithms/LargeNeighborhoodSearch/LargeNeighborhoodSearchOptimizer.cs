using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;

/// <summary>
/// Generic Large Neighborhood Search foundation following Shaw's destroy-and-repair
/// large-neighborhood principle.
/// </summary>
public sealed class LargeNeighborhoodSearchOptimizer<TSolution,TRemoved> :
    IMetaheuristic<TSolution,LargeNeighborhoodSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initial;
    private readonly ILargeNeighborhoodDestroyOperator<TSolution,TRemoved> _destroy;
    private readonly ILargeNeighborhoodRepairOperator<TSolution,TRemoved> _repair;
    private readonly ILargeNeighborhoodAcceptancePolicy _acceptance;

    public LargeNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        ILargeNeighborhoodDestroyOperator<TSolution,TRemoved> destroy,
        ILargeNeighborhoodRepairOperator<TSolution,TRemoved> repair)
        : this(
            initialSolutionGenerator,
            destroy,
            repair,
            ImprovingOnlyLargeNeighborhoodAcceptancePolicy.Instance)
    {
    }

    public LargeNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        ILargeNeighborhoodDestroyOperator<TSolution,TRemoved> destroy,
        ILargeNeighborhoodRepairOperator<TSolution,TRemoved> repair,
        ILargeNeighborhoodAcceptancePolicy acceptance)
    {
        _initial =
            initialSolutionGenerator ??
            throw new ArgumentNullException(nameof(initialSolutionGenerator));

        _destroy =
            destroy ??
            throw new ArgumentNullException(nameof(destroy));

        _repair =
            repair ??
            throw new ArgumentNullException(nameof(repair));

        _acceptance =
            acceptance ??
            throw new ArgumentNullException(nameof(acceptance));
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.LargeNeighborhoodSearch,
            Name = "Large Neighborhood Search - Shaw",
            Acronym = "LNS",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.TrajectoryBased,
            Mechanisms =
                MetaheuristicMechanism.Neighborhood |
                MetaheuristicMechanism.Trajectory |
                MetaheuristicMechanism.Constructive,
            SearchSpaces =
                SearchSpaceKind.Binary |
                SearchSpaceKind.Integer |
                SearchSpaceKind.Permutation |
                SearchSpaceKind.Combinatorial |
                SearchSpaceKind.Mixed,
            IsStochastic = true,
            References =
            [
                LargeNeighborhoodSearchReferences.Shaw1998,
                LargeNeighborhoodSearchReferences.PisingerRopke2010
            ]
        };

    public LargeNeighborhoodSearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        LargeNeighborhoodSearchParameters parameters,
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

        long destroyInvocations = 0;
        long repairInvocations = 0;
        long acceptedCandidates = 0;
        long rejectedCandidates = 0;

        var initialState =
            new LargeNeighborhoodSearchState(
                0,
                double.NaN,
                problem.Sense.WorstValue(),
                double.NaN,
                parameters.DestructionSize,
                destroyInvocations,
                repairInvocations,
                acceptedCandidates,
                rejectedCandidates);

        context.Start(initialState);

        TSolution current =
            _initial.Create(
                problem,
                context.Random);

        double currentObjective =
            context.Evaluate(
                current,
                initialState);

        RequireFiniteObjective(
            currentObjective);

        LargeNeighborhoodSearchState state =
            CreateState(
                0,
                currentObjective,
                context.State.BestFitness,
                currentObjective,
                parameters.DestructionSize,
                destroyInvocations,
                repairInvocations,
                acceptedCandidates,
                rejectedCandidates);

        StoppingDecision stop =
            context.EvaluateStopping(
                state);

        if (stop.ShouldStop)
        {
            return context.Complete(
                stop,
                state);
        }

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TSolution candidate =
                solutionCloner.Clone(
                    current);

            TRemoved removed =
                _destroy.Destroy(
                    ref candidate,
                    parameters.DestructionSize,
                    problem,
                    context.Random);

            destroyInvocations++;

            _repair.Repair(
                ref candidate,
                in removed,
                problem,
                context.Random);

            repairInvocations++;

            state =
                CreateState(
                    iteration - 1,
                    currentObjective,
                    context.State.BestFitness,
                    double.NaN,
                    parameters.DestructionSize,
                    destroyInvocations,
                    repairInvocations,
                    acceptedCandidates,
                    rejectedCandidates);

            double candidateObjective =
                context.Evaluate(
                    candidate,
                    state);

            RequireFiniteObjective(
                candidateObjective);

            state =
                CreateState(
                    iteration - 1,
                    currentObjective,
                    context.State.BestFitness,
                    candidateObjective,
                    parameters.DestructionSize,
                    destroyInvocations,
                    repairInvocations,
                    acceptedCandidates,
                    rejectedCandidates);

            stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                // The candidate was fully repaired and evaluated, but the
                // destroy-repair-accept cycle is incomplete and is not counted.
                return context.Complete(
                    stop,
                    state);
            }

            var acceptanceContext =
                new LargeNeighborhoodAcceptanceContext(
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

            state =
                CreateState(
                    iteration,
                    currentObjective,
                    context.State.BestFitness,
                    candidateObjective,
                    parameters.DestructionSize,
                    destroyInvocations,
                    repairInvocations,
                    acceptedCandidates,
                    rejectedCandidates);

            context.CompleteIteration(
                currentObjective,
                state);

            stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumLargeNeighborhoodSearchIterations",
                "The configured Large Neighborhood Search iteration limit was reached."),
            state);
    }

    private static LargeNeighborhoodSearchState CreateState(
        int iterationsCompleted,
        double currentObjective,
        double bestObjective,
        double lastCandidateObjective,
        int destructionSize,
        long destroyInvocations,
        long repairInvocations,
        long acceptedCandidates,
        long rejectedCandidates) =>
        new(
            iterationsCompleted,
            currentObjective,
            bestObjective,
            lastCandidateObjective,
            destructionSize,
            destroyInvocations,
            repairInvocations,
            acceptedCandidates,
            rejectedCandidates);

    private static void RequireFiniteObjective(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Large Neighborhood Search requires finite objective values.");
        }
    }
}
