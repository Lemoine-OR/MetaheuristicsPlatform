using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Sequential multi-start local search. Each start is generated independently and improved by the
/// same reusable local-search procedure while one common optimization context preserves exact
/// evaluation accounting, callbacks, stopping and best-so-far ownership.
/// </summary>
public sealed class MultiStartLocalSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution, MultiStartLocalSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;

    /// <summary>Creates a multi-start local-search composition.</summary>
    public MultiStartLocalSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        ILocalSearchProcedure<TSolution> localSearch)
    {
        _initialGenerator = initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _localSearch = localSearch ?? throw new ArgumentNullException(nameof(localSearch));
    }

    /// <inheritdoc />
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "multi-start-local-search",
        Name = "Multi-Start Local Search",
        Acronym = "MSLS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            NeighborhoodSearchReferences.Marti2003,
            NeighborhoodSearchReferences.Talbi2009
        }
    };

    /// <inheritdoc />
    public MultiStartLocalSearchParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        MultiStartLocalSearchParameters parameters,
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

        context.Start();

        long acceptedMoves = 0;

        for (int start = 1; start <= parameters.MaximumStarts; start++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TSolution solution = _initialGenerator.Create(problem, context.Random);
            double fitness = context.Evaluate(
                solution,
                new MultiStartLocalSearchState(start, acceptedMoves));

            StoppingDecision stop = context.EvaluateStopping(
                new MultiStartLocalSearchState(start, acceptedMoves));
            if (stop.ShouldStop)
            {
                return context.Complete(stop);
            }

            LocalSearchProcedureResult localResult = _localSearch.Improve(
                ref solution,
                fitness,
                context,
                solutionCloner,
                cancellationToken);

            acceptedMoves += localResult.AcceptedMoves;

            if (localResult.StoppingDecision.ShouldStop)
            {
                return context.Complete(localResult.StoppingDecision);
            }

            stop = context.EvaluateStopping(
                new MultiStartLocalSearchState(start, acceptedMoves));
            if (stop.ShouldStop)
            {
                return context.Complete(stop);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumStarts",
                "The configured multi-start limit was reached."),
            new MultiStartLocalSearchState(parameters.MaximumStarts, acceptedMoves));
    }
}

/// <summary>
/// Iterated Local Search following the decomposition of Lourenço, Martin and Stützle:
/// initial solution, local search, perturbation, local search and acceptance criterion.
/// </summary>
public sealed class IteratedLocalSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution, IteratedLocalSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;
    private readonly ISolutionPerturbation<TSolution> _perturbation;

    /// <summary>Creates an Iterated Local Search composition.</summary>
    public IteratedLocalSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        ILocalSearchProcedure<TSolution> localSearch,
        ISolutionPerturbation<TSolution> perturbation)
    {
        _initialGenerator = initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _localSearch = localSearch ?? throw new ArgumentNullException(nameof(localSearch));
        _perturbation = perturbation ?? throw new ArgumentNullException(nameof(perturbation));
    }

    /// <inheritdoc />
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "iterated-local-search-lourenco-martin-stutzle",
        Name = "Iterated Local Search - Lourenço-Martin-Stützle",
        Acronym = "ILS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            NeighborhoodSearchReferences.LourencoMartinStutzle2003,
            NeighborhoodSearchReferences.Talbi2009
        }
    };

    /// <inheritdoc />
    public IteratedLocalSearchParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        IteratedLocalSearchParameters parameters,
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

        context.Start();

        TSolution current = _initialGenerator.Create(problem, context.Random);
        double currentFitness = context.Evaluate(current);

        StoppingDecision stop = context.EvaluateStopping();
        if (stop.ShouldStop)
        {
            return context.Complete(stop);
        }

        LocalSearchProcedureResult initialLocalResult = _localSearch.Improve(
            ref current,
            currentFitness,
            context,
            solutionCloner,
            cancellationToken);
        currentFitness = initialLocalResult.Fitness;

        if (initialLocalResult.StoppingDecision.ShouldStop)
        {
            return context.Complete(initialLocalResult.StoppingDecision);
        }

        long acceptedLocalMoves = initialLocalResult.AcceptedMoves;
        int acceptedCandidates = 0;

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TSolution candidate = solutionCloner.Clone(current);
            _perturbation.Perturb(ref candidate, problem, context.Random);

            var state = new IteratedLocalSearchState(
                iteration,
                acceptedCandidates,
                acceptedLocalMoves,
                parameters.Acceptance);

            double candidateFitness = context.Evaluate(candidate, state);

            stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
            {
                return context.Complete(stop);
            }

            LocalSearchProcedureResult localResult = _localSearch.Improve(
                ref candidate,
                candidateFitness,
                context,
                solutionCloner,
                cancellationToken);
            candidateFitness = localResult.Fitness;
            acceptedLocalMoves += localResult.AcceptedMoves;

            if (localResult.StoppingDecision.ShouldStop)
            {
                return context.Complete(localResult.StoppingDecision);
            }

            if (ShouldAccept(
                    problem.Sense,
                    candidateFitness,
                    currentFitness,
                    parameters.Acceptance))
            {
                current = candidate;
                currentFitness = candidateFitness;
                acceptedCandidates++;
            }

            state = new IteratedLocalSearchState(
                iteration,
                acceptedCandidates,
                acceptedLocalMoves,
                parameters.Acceptance);

            stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumIlsIterations",
                "The configured iterated-local-search cycle limit was reached."),
            new IteratedLocalSearchState(
                parameters.MaximumIterations,
                acceptedCandidates,
                acceptedLocalMoves,
                parameters.Acceptance));
    }

    private static bool ShouldAccept(
        OptimizationSense sense,
        double candidateFitness,
        double currentFitness,
        NeighborhoodAcceptanceKind acceptance)
    {
        if (double.IsNaN(candidateFitness))
        {
            return false;
        }

        return acceptance switch
        {
            NeighborhoodAcceptanceKind.ImprovingOnly =>
                sense.IsBetter(candidateFitness, currentFitness),
            NeighborhoodAcceptanceKind.ImprovingOrEqual =>
                sense.IsBetter(candidateFitness, currentFitness) ||
                candidateFitness.Equals(currentFitness),
            NeighborhoodAcceptanceKind.Always => true,
            _ => throw new ArgumentOutOfRangeException(nameof(acceptance))
        };
    }
}

/// <summary>Observable algorithm state supplied to custom stopping criteria during multi-start search.</summary>
public readonly record struct MultiStartLocalSearchState(
    int StartsCompleted,
    long AcceptedLocalMoves);

/// <summary>Observable algorithm state supplied to custom stopping criteria during ILS.</summary>
public readonly record struct IteratedLocalSearchState(
    int IterationsCompleted,
    int AcceptedCandidates,
    long AcceptedLocalMoves,
    NeighborhoodAcceptanceKind Acceptance);
