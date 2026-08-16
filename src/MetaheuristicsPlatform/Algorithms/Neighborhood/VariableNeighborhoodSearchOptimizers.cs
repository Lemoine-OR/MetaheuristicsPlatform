using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Standalone Variable Neighborhood Descent (VND) over an ordered sequence of reusable
/// local-search procedures.
/// </summary>
public sealed class VariableNeighborhoodDescentOptimizer<TSolution> :
    IMetaheuristic<TSolution, VariableNeighborhoodDescentParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ILocalSearchProcedure<TSolution>[] _neighborhoods;

    /// <summary>Creates a standalone Variable Neighborhood Descent optimizer.</summary>
    public VariableNeighborhoodDescentOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IReadOnlyList<ILocalSearchProcedure<TSolution>> neighborhoods)
    {
        _initialGenerator =
            initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));

        ArgumentNullException.ThrowIfNull(neighborhoods);

        if (neighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one local-search neighborhood is required.",
                nameof(neighborhoods));
        }

        _neighborhoods = new ILocalSearchProcedure<TSolution>[neighborhoods.Count];
        for (int i = 0; i < neighborhoods.Count; i++)
        {
            _neighborhoods[i] = neighborhoods[i] ??
                throw new ArgumentException(
                    "Local-search neighborhoods must not contain null entries.",
                    nameof(neighborhoods));
        }
    }

    /// <inheritdoc />
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "variable-neighborhood-descent",
        Name = "Variable Neighborhood Descent",
        Acronym = "VND",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = false,
        References = new[]
        {
            VariableNeighborhoodSearchReferences.MladenovicHansen1997,
            VariableNeighborhoodSearchReferences.HansenMladenovic2001
        }
    };

    /// <inheritdoc />
    public VariableNeighborhoodDescentParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        VariableNeighborhoodDescentParameters parameters,
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

        TSolution solution = _initialGenerator.Create(problem, context.Random);
        double fitness = context.Evaluate(solution);

        StoppingDecision stop = context.EvaluateStopping();
        if (stop.ShouldStop)
        {
            return context.Complete(stop);
        }

        var procedure = new VariableNeighborhoodDescentProcedure<TSolution>(
            _neighborhoods,
            parameters.MaximumNeighborhoodRestarts);

        LocalSearchProcedureResult result = procedure.Improve(
            ref solution,
            fitness,
            context,
            solutionCloner,
            cancellationToken);

        if (result.StoppingDecision.ShouldStop)
        {
            return context.Complete(result.StoppingDecision);
        }

        if (result.IsLocalOptimum)
        {
            return context.Complete(
                StoppingDecision.Stop(
                    "VariableNeighborhoodLocalOptimum",
                    "No strict improvement remains in any configured VND neighborhood."));
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumNeighborhoodRestarts",
                "The configured VND neighborhood-restart safety cap was reached."));
    }
}

/// <summary>
/// Canonical basic Variable Neighborhood Search (VNS) of Mladenovic and Hansen.
/// The optimizer systematically changes the shaking neighborhood, applies a reusable
/// local-search procedure, and restarts at the first neighborhood after a strict improvement.
/// </summary>
public sealed class VariableNeighborhoodSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution, VariableNeighborhoodSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ISolutionPerturbation<TSolution>[] _shakingNeighborhoods;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;

    /// <summary>Creates a canonical basic Variable Neighborhood Search composition.</summary>
    public VariableNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods,
        ILocalSearchProcedure<TSolution> localSearch)
    {
        _initialGenerator =
            initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _localSearch =
            localSearch ?? throw new ArgumentNullException(nameof(localSearch));

        ArgumentNullException.ThrowIfNull(shakingNeighborhoods);

        if (shakingNeighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one shaking neighborhood is required.",
                nameof(shakingNeighborhoods));
        }

        _shakingNeighborhoods =
            new ISolutionPerturbation<TSolution>[shakingNeighborhoods.Count];

        for (int i = 0; i < shakingNeighborhoods.Count; i++)
        {
            _shakingNeighborhoods[i] = shakingNeighborhoods[i] ??
                throw new ArgumentException(
                    "Shaking neighborhoods must not contain null entries.",
                    nameof(shakingNeighborhoods));
        }
    }

    /// <inheritdoc />
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "variable-neighborhood-search-mladenovic-hansen",
        Name = "Variable Neighborhood Search - Mladenovic-Hansen",
        Acronym = "VNS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary | SearchSpaceKind.Integer |
                       SearchSpaceKind.Permutation | SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            VariableNeighborhoodSearchReferences.MladenovicHansen1997,
            VariableNeighborhoodSearchReferences.HansenMladenovic2001
        }
    };

    /// <inheritdoc />
    public VariableNeighborhoodSearchParameters CreateDefaultParameters() => new();

    /// <inheritdoc />
    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        VariableNeighborhoodSearchParameters parameters,
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

        int acceptedCandidates = 0;
        long acceptedLocalMoves = 0;

        for (int cycle = 1; cycle <= parameters.MaximumCycles; cycle++)
        {
            int neighborhoodIndex = 0;

            while (neighborhoodIndex < _shakingNeighborhoods.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var state = new VariableNeighborhoodSearchState(
                    CyclesCompleted: cycle - 1,
                    NeighborhoodIndex: neighborhoodIndex + 1,
                    NeighborhoodCount: _shakingNeighborhoods.Length,
                    AcceptedCandidates: acceptedCandidates,
                    AcceptedLocalMoves: acceptedLocalMoves);

                TSolution candidate = solutionCloner.Clone(current);

                _shakingNeighborhoods[neighborhoodIndex].Perturb(
                    ref candidate,
                    problem,
                    context.Random);

                double candidateFitness = context.Evaluate(candidate, state);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
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

                if (problem.Sense.IsBetter(candidateFitness, currentFitness))
                {
                    current = candidate;
                    currentFitness = candidateFitness;
                    acceptedCandidates++;
                    neighborhoodIndex = 0;
                }
                else
                {
                    neighborhoodIndex++;
                }

                state = new VariableNeighborhoodSearchState(
                    CyclesCompleted: cycle - 1,
                    NeighborhoodIndex:
                        Math.Min(neighborhoodIndex + 1, _shakingNeighborhoods.Length),
                    NeighborhoodCount: _shakingNeighborhoods.Length,
                    AcceptedCandidates: acceptedCandidates,
                    AcceptedLocalMoves: acceptedLocalMoves);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumVnsCycles",
                "The configured Variable Neighborhood Search cycle limit was reached."),
            new VariableNeighborhoodSearchState(
                CyclesCompleted: parameters.MaximumCycles,
                NeighborhoodIndex: _shakingNeighborhoods.Length,
                NeighborhoodCount: _shakingNeighborhoods.Length,
                AcceptedCandidates: acceptedCandidates,
                AcceptedLocalMoves: acceptedLocalMoves));
    }
}

/// <summary>Observable state supplied to stopping criteria during Variable Neighborhood Search.</summary>
public readonly record struct VariableNeighborhoodSearchState(
    int CyclesCompleted,
    int NeighborhoodIndex,
    int NeighborhoodCount,
    int AcceptedCandidates,
    long AcceptedLocalMoves);
