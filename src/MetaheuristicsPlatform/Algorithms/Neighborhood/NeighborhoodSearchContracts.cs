using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Creates a starting solution for neighborhood-based metaheuristics.</summary>
public interface INeighborhoodSearchInitialSolutionGenerator<TSolution>
{
    TSolution Create(IOptimizationProblem<TSolution> problem, IRandomSource random);
}

/// <summary>Delegate-backed initial solution generator.</summary>
public sealed class DelegateNeighborhoodSearchInitialSolutionGenerator<TSolution> :
    INeighborhoodSearchInitialSolutionGenerator<TSolution>
{
    private readonly Func<IOptimizationProblem<TSolution>, IRandomSource, TSolution> _factory;

    public DelegateNeighborhoodSearchInitialSolutionGenerator(
        Func<IOptimizationProblem<TSolution>, IRandomSource, TSolution> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public TSolution Create(IOptimizationProblem<TSolution> problem, IRandomSource random) =>
        _factory(problem, random);
}

/// <summary>Search policy used inside move-based local search.</summary>
public enum LocalSearchSelectionPolicy
{
    FirstImprovement = 0,
    BestImprovement = 1
}

/// <summary>Result returned by a reusable local-search procedure.</summary>
public readonly struct LocalSearchProcedureResult
{
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

    public double Fitness { get; }
    public long AcceptedMoves { get; }
    public bool IsLocalOptimum { get; }
    public StoppingDecision StoppingDecision { get; }
}

/// <summary>
/// Reusable local-improvement procedure. Implementations use the common OptimizationContext
/// so objective probes, callbacks, cancellation, best-so-far state and stopping stay exact.
/// </summary>
public interface ILocalSearchProcedure<TSolution>
{
    LocalSearchProcedureResult Improve(
        ref TSolution solution,
        double currentFitness,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken);
}
