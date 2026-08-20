using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

/// <summary>
/// Partially destroys an owned complete solution and returns the information required
/// to reconstruct it. The partially destroyed solution is never evaluated by the core.
/// </summary>
public interface IIteratedGreedyDestruction<TSolution,TRemoved>
{
    TRemoved Destroy(
        ref TSolution partialSolution,
        int destructionSize,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>
/// Reconstructs a complete solution from a partially destroyed owned solution and the
/// removed-component state returned by the destruction operator.
/// </summary>
public interface IIteratedGreedyConstruction<TSolution,TRemoved>
{
    void Reconstruct(
        ref TSolution partialSolution,
        in TRemoved removedComponents,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

public delegate TRemoved IteratedGreedyDestructionDelegate<TSolution,TRemoved>(
    ref TSolution partialSolution,
    int destructionSize,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public delegate void IteratedGreedyConstructionDelegate<TSolution,TRemoved>(
    ref TSolution partialSolution,
    in TRemoved removedComponents,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

/// <summary>Delegate-backed Iterated Greedy destruction operator.</summary>
public sealed class DelegateIteratedGreedyDestruction<TSolution,TRemoved> :
    IIteratedGreedyDestruction<TSolution,TRemoved>
{
    private readonly IteratedGreedyDestructionDelegate<TSolution,TRemoved> _destruction;

    public DelegateIteratedGreedyDestruction(
        IteratedGreedyDestructionDelegate<TSolution,TRemoved> destruction)
    {
        _destruction = destruction ?? throw new ArgumentNullException(nameof(destruction));
    }

    public TRemoved Destroy(
        ref TSolution partialSolution,
        int destructionSize,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _destruction(ref partialSolution, destructionSize, problem, random);
}

/// <summary>Delegate-backed Iterated Greedy reconstruction operator.</summary>
public sealed class DelegateIteratedGreedyConstruction<TSolution,TRemoved> :
    IIteratedGreedyConstruction<TSolution,TRemoved>
{
    private readonly IteratedGreedyConstructionDelegate<TSolution,TRemoved> _construction;

    public DelegateIteratedGreedyConstruction(
        IteratedGreedyConstructionDelegate<TSolution,TRemoved> construction)
    {
        _construction = construction ?? throw new ArgumentNullException(nameof(construction));
    }

    public void Reconstruct(
        ref TSolution partialSolution,
        in TRemoved removedComponents,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _construction(ref partialSolution, in removedComponents, problem, random);
}
