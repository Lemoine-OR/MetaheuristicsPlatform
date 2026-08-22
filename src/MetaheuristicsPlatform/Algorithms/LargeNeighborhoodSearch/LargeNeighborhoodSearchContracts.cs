using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;

/// <summary>
/// Destroys part of an owned candidate solution and returns domain-owned information
/// required by the matching repair operator.
/// </summary>
public interface ILargeNeighborhoodDestroyOperator<TSolution,TRemoved>
{
    TRemoved Destroy(
        ref TSolution partialSolution,
        int destructionSize,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>
/// Restores a complete evaluable solution after a Large Neighborhood Search destruction.
/// </summary>
public interface ILargeNeighborhoodRepairOperator<TSolution,TRemoved>
{
    void Repair(
        ref TSolution partialSolution,
        in TRemoved removedComponents,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>Delegate signature for a domain-defined LNS destruction operator.</summary>
public delegate TRemoved LargeNeighborhoodDestroyDelegate<TSolution,TRemoved>(
    ref TSolution partialSolution,
    int destructionSize,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

/// <summary>Delegate-backed LNS destruction operator.</summary>
public sealed class DelegateLargeNeighborhoodDestroyOperator<TSolution,TRemoved> :
    ILargeNeighborhoodDestroyOperator<TSolution,TRemoved>
{
    private readonly LargeNeighborhoodDestroyDelegate<TSolution,TRemoved> _destroy;

    public DelegateLargeNeighborhoodDestroyOperator(
        LargeNeighborhoodDestroyDelegate<TSolution,TRemoved> destroy)
    {
        _destroy =
            destroy ??
            throw new ArgumentNullException(nameof(destroy));
    }

    public TRemoved Destroy(
        ref TSolution partialSolution,
        int destructionSize,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _destroy(
            ref partialSolution,
            destructionSize,
            problem,
            random);
}

/// <summary>Delegate signature for a domain-defined LNS repair operator.</summary>
public delegate void LargeNeighborhoodRepairDelegate<TSolution,TRemoved>(
    ref TSolution partialSolution,
    in TRemoved removedComponents,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

/// <summary>Delegate-backed LNS repair operator.</summary>
public sealed class DelegateLargeNeighborhoodRepairOperator<TSolution,TRemoved> :
    ILargeNeighborhoodRepairOperator<TSolution,TRemoved>
{
    private readonly LargeNeighborhoodRepairDelegate<TSolution,TRemoved> _repair;

    public DelegateLargeNeighborhoodRepairOperator(
        LargeNeighborhoodRepairDelegate<TSolution,TRemoved> repair)
    {
        _repair =
            repair ??
            throw new ArgumentNullException(nameof(repair));
    }

    public void Repair(
        ref TSolution partialSolution,
        in TRemoved removedComponents,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _repair(
            ref partialSolution,
            in removedComponents,
            problem,
            random);
}

/// <summary>Context supplied to an LNS incumbent-acceptance policy.</summary>
public readonly record struct LargeNeighborhoodAcceptanceContext(
    OptimizationSense Sense,
    int Iteration,
    double CurrentObjective,
    double CandidateObjective,
    double BestObjective);

/// <summary>
/// Decides whether a fully repaired and evaluated LNS candidate replaces the incumbent.
/// </summary>
public interface ILargeNeighborhoodAcceptancePolicy
{
    bool ShouldAccept(
        in LargeNeighborhoodAcceptanceContext context,
        IRandomSource random);
}

/// <summary>
/// Canonical deterministic LNS acceptance: replace the incumbent only on strict
/// objective improvement.
/// </summary>
public sealed class ImprovingOnlyLargeNeighborhoodAcceptancePolicy :
    ILargeNeighborhoodAcceptancePolicy
{
    public static ImprovingOnlyLargeNeighborhoodAcceptancePolicy Instance { get; } =
        new();

    private ImprovingOnlyLargeNeighborhoodAcceptancePolicy()
    {
    }

    public bool ShouldAccept(
        in LargeNeighborhoodAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        return context.Sense.IsBetter(
            context.CandidateObjective,
            context.CurrentObjective);
    }
}
