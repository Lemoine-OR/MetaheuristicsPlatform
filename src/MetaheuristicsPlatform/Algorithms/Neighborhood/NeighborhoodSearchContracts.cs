using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Creates a starting solution for neighborhood-based metaheuristics.</summary>
public interface INeighborhoodSearchInitialSolutionGenerator<TSolution>
{
    /// <summary>Creates one starting solution.</summary>
    TSolution Create(IOptimizationProblem<TSolution> problem, IRandomSource random);
}

/// <summary>Delegate-backed initial solution generator.</summary>
public sealed class DelegateNeighborhoodSearchInitialSolutionGenerator<TSolution> :
    INeighborhoodSearchInitialSolutionGenerator<TSolution>
{
    private readonly Func<IOptimizationProblem<TSolution>, IRandomSource, TSolution> _factory;

    /// <summary>Creates a delegate-backed generator.</summary>
    public DelegateNeighborhoodSearchInitialSolutionGenerator(
        Func<IOptimizationProblem<TSolution>, IRandomSource, TSolution> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public TSolution Create(IOptimizationProblem<TSolution> problem, IRandomSource random) =>
        _factory(problem, random);
}

/// <summary>Search policy used inside move-based local search.</summary>
public enum LocalSearchSelectionPolicy
{
    /// <summary>Accept the first strict improving move in enumeration order.</summary>
    FirstImprovement = 0,

    /// <summary>Scan the complete neighborhood and accept its best strict improvement.</summary>
    BestImprovement = 1
}

/// <summary>
/// Built-in acceptance rules for iterated local search. The best-so-far solution remains
/// managed independently by <see cref="OptimizationContext{TSolution}"/>.
/// </summary>
public enum NeighborhoodAcceptanceKind
{
    /// <summary>Accept a locally improved candidate only when it strictly improves the incumbent.</summary>
    ImprovingOnly = 0,

    /// <summary>Accept when the candidate improves or exactly ties the incumbent objective.</summary>
    ImprovingOrEqual = 1,

    /// <summary>Accept every non-NaN candidate, allowing deliberate diversification.</summary>
    Always = 2
}

/// <summary>
/// Domain-owned perturbation used by iterated local search to leave the current local optimum basin.
/// The supplied solution is an owned clone and may therefore be mutated in place.
/// </summary>
public interface ISolutionPerturbation<TSolution>
{
    /// <summary>Perturbs an owned candidate solution in place.</summary>
    void Perturb(
        ref TSolution solution,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>Delegate signature for a domain-owned ILS perturbation.</summary>
public delegate void SolutionPerturbationDelegate<TSolution>(
    ref TSolution solution,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

/// <summary>Delegate-backed solution perturbation.</summary>
public sealed class DelegateSolutionPerturbation<TSolution> : ISolutionPerturbation<TSolution>
{
    private readonly SolutionPerturbationDelegate<TSolution> _perturbation;

    /// <summary>Creates a delegate-backed perturbation.</summary>
    public DelegateSolutionPerturbation(SolutionPerturbationDelegate<TSolution> perturbation)
    {
        _perturbation = perturbation ?? throw new ArgumentNullException(nameof(perturbation));
    }

    /// <inheritdoc />
    public void Perturb(
        ref TSolution solution,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _perturbation(ref solution, problem, random);
}

/// <summary>Result returned by a reusable local-search procedure.</summary>
public readonly struct LocalSearchProcedureResult
{
    /// <summary>Creates a local-search procedure result.</summary>
    public LocalSearchProcedureResult(
        double fitness,
        long acceptedMoves,
        bool localOptimum,
        StoppingDecision stoppingDecision)
    {
        Fitness = fitness;
        AcceptedMoves = acceptedMoves;
        IsLocalOptimum = localOptimum;
        StoppingDecision = stoppingDecision;
    }

    /// <summary>Objective value of the returned solution.</summary>
    public double Fitness { get; }

    /// <summary>Number of improving moves accepted during this invocation.</summary>
    public long AcceptedMoves { get; }

    /// <summary>Whether the procedure proved that no strict improving move remains.</summary>
    public bool IsLocalOptimum { get; }

    /// <summary>Common stopping decision observed while improving the solution.</summary>
    public StoppingDecision StoppingDecision { get; }
}

/// <summary>
/// Reusable local-improvement procedure. Implementations use the common OptimizationContext
/// so objective probes, callbacks, cancellation, best-so-far state and stopping stay exact.
/// </summary>
public interface ILocalSearchProcedure<TSolution>
{
    /// <summary>Improves <paramref name="solution"/> in place from its known objective value.</summary>
    LocalSearchProcedureResult Improve(
        ref TSolution solution,
        double currentFitness,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken);
}
