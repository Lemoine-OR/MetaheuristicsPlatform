using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Reduced Variable Neighborhood Search (RVNS): shaking plus neighborhood change,
/// deliberately omitting the local-improvement phase.
/// </summary>
public sealed class ReducedVariableNeighborhoodSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution, ReducedVariableNeighborhoodSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ISolutionPerturbation<TSolution>[] _shakingNeighborhoods;

    public ReducedVariableNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods)
    {
        _initialGenerator =
            initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _shakingNeighborhoods = CopyShakingNeighborhoods(shakingNeighborhoods);
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "reduced-variable-neighborhood-search",
        Name = "Reduced Variable Neighborhood Search",
        Acronym = "RVNS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary |
                       SearchSpaceKind.Integer | SearchSpaceKind.Permutation |
                       SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            VariableNeighborhoodSearchReferences.MladenovicHansen1997,
            VariableNeighborhoodSearchReferences.HansenMladenovic2001,
            AdvancedVariableNeighborhoodSearchReferences.HansenMladenovicTodosijevicHanafi2017
        }
    };

    public ReducedVariableNeighborhoodSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        ReducedVariableNeighborhoodSearchParameters parameters,
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

        TSolution current = _initialGenerator.Create(problem, context.Random);
        double currentFitness = context.Evaluate(current);

        StoppingDecision stop = context.EvaluateStopping();
        if (stop.ShouldStop)
        {
            return context.Complete(stop);
        }

        int acceptedCandidates = 0;

        for (int cycle = 1; cycle <= parameters.MaximumCycles; cycle++)
        {
            int neighborhoodIndex = 0;

            while (neighborhoodIndex < _shakingNeighborhoods.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AdvancedVariableNeighborhoodSearchState state = CreateState(
                    "RVNS", cycle - 1, neighborhoodIndex,
                    _shakingNeighborhoods.Length, acceptedCandidates, 0, 0);

                TSolution candidate = solutionCloner.Clone(current);
                _shakingNeighborhoods[neighborhoodIndex].Perturb(
                    ref candidate, problem, context.Random);

                double candidateFitness = context.Evaluate(candidate, state);

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

                state = CreateState(
                    "RVNS", cycle - 1, neighborhoodIndex,
                    _shakingNeighborhoods.Length, acceptedCandidates, 0, 0);

                // RVNS has no inner local-search moves, so one shaking decision is
                // the natural common iteration unit for callbacks/iteration stops.
                context.CompleteIteration(currentFitness, state);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumReducedVnsCycles",
                "The configured Reduced VNS cycle limit was reached."),
            CreateState(
                "RVNS", parameters.MaximumCycles, _shakingNeighborhoods.Length,
                _shakingNeighborhoods.Length, acceptedCandidates, 0, 0));
    }

    private static ISolutionPerturbation<TSolution>[] CopyShakingNeighborhoods(
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods)
    {
        ArgumentNullException.ThrowIfNull(shakingNeighborhoods);

        if (shakingNeighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one shaking neighborhood is required.",
                nameof(shakingNeighborhoods));
        }

        var copy =
            new ISolutionPerturbation<TSolution>[shakingNeighborhoods.Count];

        for (int i = 0; i < shakingNeighborhoods.Count; i++)
        {
            copy[i] = shakingNeighborhoods[i] ??
                throw new ArgumentException(
                    "Shaking neighborhoods must not contain null entries.",
                    nameof(shakingNeighborhoods));
        }

        return copy;
    }

    private static AdvancedVariableNeighborhoodSearchState CreateState(
        string variant,
        int cyclesCompleted,
        int neighborhoodIndex,
        int neighborhoodCount,
        int acceptedCandidates,
        long acceptedLocalMoves,
        int skewedAcceptances) =>
        new(
            variant,
            cyclesCompleted,
            Math.Min(neighborhoodIndex + 1, neighborhoodCount),
            neighborhoodCount,
            acceptedCandidates,
            acceptedLocalMoves,
            skewedAcceptances);
}

/// <summary>
/// General Variable Neighborhood Search (GVNS): VNS shaking followed by reusable VND.
/// </summary>
public sealed class GeneralVariableNeighborhoodSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution, GeneralVariableNeighborhoodSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ISolutionPerturbation<TSolution>[] _shakingNeighborhoods;
    private readonly ILocalSearchProcedure<TSolution>[] _localSearchNeighborhoods;

    public GeneralVariableNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods,
        IReadOnlyList<ILocalSearchProcedure<TSolution>> localSearchNeighborhoods)
    {
        _initialGenerator =
            initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _shakingNeighborhoods = CopyShakingNeighborhoods(shakingNeighborhoods);

        ArgumentNullException.ThrowIfNull(localSearchNeighborhoods);

        if (localSearchNeighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one VND local-search neighborhood is required.",
                nameof(localSearchNeighborhoods));
        }

        _localSearchNeighborhoods =
            new ILocalSearchProcedure<TSolution>[localSearchNeighborhoods.Count];

        for (int i = 0; i < localSearchNeighborhoods.Count; i++)
        {
            _localSearchNeighborhoods[i] = localSearchNeighborhoods[i] ??
                throw new ArgumentException(
                    "VND local-search neighborhoods must not contain null entries.",
                    nameof(localSearchNeighborhoods));
        }
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "general-variable-neighborhood-search",
        Name = "General Variable Neighborhood Search",
        Acronym = "GVNS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary |
                       SearchSpaceKind.Integer | SearchSpaceKind.Permutation |
                       SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            VariableNeighborhoodSearchReferences.HansenMladenovic2001,
            AdvancedVariableNeighborhoodSearchReferences.HansenMladenovicTodosijevicHanafi2017
        }
    };

    public GeneralVariableNeighborhoodSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        GeneralVariableNeighborhoodSearchParameters parameters,
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

        TSolution current = _initialGenerator.Create(problem, context.Random);
        double currentFitness = context.Evaluate(current);

        StoppingDecision stop = context.EvaluateStopping();
        if (stop.ShouldStop)
        {
            return context.Complete(stop);
        }

        var vnd = new VariableNeighborhoodDescentProcedure<TSolution>(
            _localSearchNeighborhoods,
            parameters.MaximumNeighborhoodRestarts);

        int acceptedCandidates = 0;
        long acceptedLocalMoves = 0;

        for (int cycle = 1; cycle <= parameters.MaximumCycles; cycle++)
        {
            int neighborhoodIndex = 0;

            while (neighborhoodIndex < _shakingNeighborhoods.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AdvancedVariableNeighborhoodSearchState state = CreateState(
                    "GVNS", cycle - 1, neighborhoodIndex,
                    _shakingNeighborhoods.Length,
                    acceptedCandidates, acceptedLocalMoves, 0);

                TSolution candidate = solutionCloner.Clone(current);
                _shakingNeighborhoods[neighborhoodIndex].Perturb(
                    ref candidate, problem, context.Random);

                double candidateFitness = context.Evaluate(candidate, state);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }

                LocalSearchProcedureResult localResult = vnd.Improve(
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

                state = CreateState(
                    "GVNS", cycle - 1, neighborhoodIndex,
                    _shakingNeighborhoods.Length,
                    acceptedCandidates, acceptedLocalMoves, 0);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumGeneralVnsCycles",
                "The configured General VNS cycle limit was reached."),
            CreateState(
                "GVNS", parameters.MaximumCycles, _shakingNeighborhoods.Length,
                _shakingNeighborhoods.Length,
                acceptedCandidates, acceptedLocalMoves, 0));
    }

    private static ISolutionPerturbation<TSolution>[] CopyShakingNeighborhoods(
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods)
    {
        ArgumentNullException.ThrowIfNull(shakingNeighborhoods);

        if (shakingNeighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one shaking neighborhood is required.",
                nameof(shakingNeighborhoods));
        }

        var copy =
            new ISolutionPerturbation<TSolution>[shakingNeighborhoods.Count];

        for (int i = 0; i < shakingNeighborhoods.Count; i++)
        {
            copy[i] = shakingNeighborhoods[i] ??
                throw new ArgumentException(
                    "Shaking neighborhoods must not contain null entries.",
                    nameof(shakingNeighborhoods));
        }

        return copy;
    }

    private static AdvancedVariableNeighborhoodSearchState CreateState(
        string variant,
        int cyclesCompleted,
        int neighborhoodIndex,
        int neighborhoodCount,
        int acceptedCandidates,
        long acceptedLocalMoves,
        int skewedAcceptances) =>
        new(
            variant,
            cyclesCompleted,
            Math.Min(neighborhoodIndex + 1, neighborhoodCount),
            neighborhoodCount,
            acceptedCandidates,
            acceptedLocalMoves,
            skewedAcceptances);
}

/// <summary>
/// Skewed Variable Neighborhood Search (SVNS): permits sufficiently distant
/// recentering while the common context independently protects best-so-far.
/// </summary>
public sealed class SkewedVariableNeighborhoodSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution, SkewedVariableNeighborhoodSearchParameters>
{
    private readonly INeighborhoodSearchInitialSolutionGenerator<TSolution> _initialGenerator;
    private readonly ISolutionPerturbation<TSolution>[] _shakingNeighborhoods;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;
    private readonly ISolutionDistance<TSolution> _distance;

    public SkewedVariableNeighborhoodSearchOptimizer(
        INeighborhoodSearchInitialSolutionGenerator<TSolution> initialGenerator,
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods,
        ILocalSearchProcedure<TSolution> localSearch,
        ISolutionDistance<TSolution> distance)
    {
        _initialGenerator =
            initialGenerator ?? throw new ArgumentNullException(nameof(initialGenerator));
        _localSearch =
            localSearch ?? throw new ArgumentNullException(nameof(localSearch));
        _distance =
            distance ?? throw new ArgumentNullException(nameof(distance));
        _shakingNeighborhoods = CopyShakingNeighborhoods(shakingNeighborhoods);
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "skewed-variable-neighborhood-search-hansen-mladenovic-2001",
        Name = "Skewed Variable Neighborhood Search - Hansen-Mladenovic",
        Acronym = "SVNS",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory,
        SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary |
                       SearchSpaceKind.Integer | SearchSpaceKind.Permutation |
                       SearchSpaceKind.Combinatorial | SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = new[]
        {
            VariableNeighborhoodSearchReferences.HansenMladenovic2001,
            AdvancedVariableNeighborhoodSearchReferences.HansenMladenovicTodosijevicHanafi2017
        }
    };

    public SkewedVariableNeighborhoodSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        SkewedVariableNeighborhoodSearchParameters parameters,
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

        TSolution current = _initialGenerator.Create(problem, context.Random);
        double currentFitness = context.Evaluate(current);

        StoppingDecision stop = context.EvaluateStopping();
        if (stop.ShouldStop)
        {
            return context.Complete(stop);
        }

        int acceptedCandidates = 0;
        long acceptedLocalMoves = 0;
        int skewedAcceptances = 0;

        for (int cycle = 1; cycle <= parameters.MaximumCycles; cycle++)
        {
            int neighborhoodIndex = 0;

            while (neighborhoodIndex < _shakingNeighborhoods.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AdvancedVariableNeighborhoodSearchState state = CreateState(
                    "SVNS", cycle - 1, neighborhoodIndex,
                    _shakingNeighborhoods.Length,
                    acceptedCandidates, acceptedLocalMoves, skewedAcceptances);

                TSolution candidate = solutionCloner.Clone(current);
                _shakingNeighborhoods[neighborhoodIndex].Perturb(
                    ref candidate, problem, context.Random);

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

                bool accept =
                    problem.Sense.IsBetter(candidateFitness, currentFitness);

                if (!accept)
                {
                    double distance =
                        _distance.Distance(in current, in candidate);

                    if (!double.IsFinite(distance) || distance < 0.0)
                    {
                        throw new InvalidOperationException(
                            "SVNS solution distance must be finite and non-negative.");
                    }

                    double skewedCandidate =
                        problem.Sense == OptimizationSense.Minimize
                            ? candidateFitness - parameters.Alpha * distance
                            : candidateFitness + parameters.Alpha * distance;

                    accept =
                        problem.Sense.IsBetter(skewedCandidate, currentFitness);

                    if (accept)
                    {
                        skewedAcceptances++;
                    }
                }

                if (accept)
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

                state = CreateState(
                    "SVNS", cycle - 1, neighborhoodIndex,
                    _shakingNeighborhoods.Length,
                    acceptedCandidates, acceptedLocalMoves, skewedAcceptances);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumSkewedVnsCycles",
                "The configured Skewed VNS cycle limit was reached."),
            CreateState(
                "SVNS", parameters.MaximumCycles, _shakingNeighborhoods.Length,
                _shakingNeighborhoods.Length,
                acceptedCandidates, acceptedLocalMoves, skewedAcceptances));
    }

    private static ISolutionPerturbation<TSolution>[] CopyShakingNeighborhoods(
        IReadOnlyList<ISolutionPerturbation<TSolution>> shakingNeighborhoods)
    {
        ArgumentNullException.ThrowIfNull(shakingNeighborhoods);

        if (shakingNeighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one shaking neighborhood is required.",
                nameof(shakingNeighborhoods));
        }

        var copy =
            new ISolutionPerturbation<TSolution>[shakingNeighborhoods.Count];

        for (int i = 0; i < shakingNeighborhoods.Count; i++)
        {
            copy[i] = shakingNeighborhoods[i] ??
                throw new ArgumentException(
                    "Shaking neighborhoods must not contain null entries.",
                    nameof(shakingNeighborhoods));
        }

        return copy;
    }

    private static AdvancedVariableNeighborhoodSearchState CreateState(
        string variant,
        int cyclesCompleted,
        int neighborhoodIndex,
        int neighborhoodCount,
        int acceptedCandidates,
        long acceptedLocalMoves,
        int skewedAcceptances) =>
        new(
            variant,
            cyclesCompleted,
            Math.Min(neighborhoodIndex + 1, neighborhoodCount),
            neighborhoodCount,
            acceptedCandidates,
            acceptedLocalMoves,
            skewedAcceptances);
}
