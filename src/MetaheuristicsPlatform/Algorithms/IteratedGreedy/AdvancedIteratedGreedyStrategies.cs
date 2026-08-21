using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

/// <summary>
/// State exposed to an Iterated Greedy destruction-size controller.
/// ConsecutiveNonImprovingIterations counts complete IG cycles that did not improve
/// the run-wide best-so-far solution.
/// </summary>
public readonly record struct IteratedGreedyDestructionSizeContext(
    OptimizationSense Sense,
    long Iteration,
    int BaseDestructionSize,
    int ConsecutiveNonImprovingIterations,
    double CurrentObjective,
    double BestObjective);

/// <summary>
/// Selects the destruction size used by one Iterated Greedy cycle.
/// </summary>
public interface IIteratedGreedyDestructionSizePolicy
{
    int SelectDestructionSize(
        in IteratedGreedyDestructionSizeContext context);
}

/// <summary>
/// Preserves the canonical fixed destruction size supplied by IteratedGreedyParameters.
/// </summary>
public sealed class FixedIteratedGreedyDestructionSizePolicy :
    IIteratedGreedyDestructionSizePolicy
{
    public static FixedIteratedGreedyDestructionSizePolicy Instance { get; } = new();

    private FixedIteratedGreedyDestructionSizePolicy()
    {
    }

    public int SelectDestructionSize(
        in IteratedGreedyDestructionSizeContext context)
    {
        if (context.BaseDestructionSize <= 0)
            throw new InvalidOperationException("The base destruction size must be positive.");

        return context.BaseDestructionSize;
    }
}

/// <summary>
/// Generic stagnation-responsive destruction-size controller.
/// This is reusable platform infrastructure inspired by adaptive-destruction IG research;
/// it is not claimed to reproduce any one problem-specific published AIG formula.
/// </summary>
public sealed class StagnationEscalatingIteratedGreedyDestructionSizePolicy :
    IIteratedGreedyDestructionSizePolicy
{
    public StagnationEscalatingIteratedGreedyDestructionSizePolicy(
        int minimumDestructionSize,
        int maximumDestructionSize,
        int stagnationWindow,
        int increment = 1)
    {
        if (minimumDestructionSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumDestructionSize));

        if (maximumDestructionSize < minimumDestructionSize)
            throw new ArgumentOutOfRangeException(nameof(maximumDestructionSize));

        if (stagnationWindow <= 0)
            throw new ArgumentOutOfRangeException(nameof(stagnationWindow));

        if (increment <= 0)
            throw new ArgumentOutOfRangeException(nameof(increment));

        MinimumDestructionSize = minimumDestructionSize;
        MaximumDestructionSize = maximumDestructionSize;
        StagnationWindow = stagnationWindow;
        Increment = increment;
    }

    public int MinimumDestructionSize { get; }
    public int MaximumDestructionSize { get; }
    public int StagnationWindow { get; }
    public int Increment { get; }

    public int SelectDestructionSize(
        in IteratedGreedyDestructionSizeContext context)
    {
        if (context.ConsecutiveNonImprovingIterations < 0)
            throw new ArgumentOutOfRangeException(
                nameof(context),
                "The stagnation counter cannot be negative.");

        long levels =
            context.ConsecutiveNonImprovingIterations /
            (long)StagnationWindow;

        long requested =
            MinimumDestructionSize +
            levels * Increment;

        return (int)Math.Min(
            MaximumDestructionSize,
            requested);
    }
}

public delegate void IteratedGreedyPartialSolutionImprovementDelegate<TSolution,TRemoved>(
    ref TSolution partialSolution,
    in TRemoved removedComponents,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random,
    CancellationToken cancellationToken);

/// <summary>
/// Optional advanced hook executed after destruction and before reconstruction.
/// The common objective evaluator is intentionally not exposed because a generic platform
/// cannot assume that a partial solution has the same objective semantics as a complete one.
/// Domain implementations may evaluate/optimize the partial representation using their own
/// scientifically valid partial-solution model.
/// </summary>
public interface IIteratedGreedyPartialSolutionImprovement<TSolution,TRemoved>
{
    void Improve(
        ref TSolution partialSolution,
        in TRemoved removedComponents,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        CancellationToken cancellationToken);
}

/// <summary>Delegate-backed partial-solution improvement hook.</summary>
public sealed class DelegateIteratedGreedyPartialSolutionImprovement<TSolution,TRemoved> :
    IIteratedGreedyPartialSolutionImprovement<TSolution,TRemoved>
{
    private readonly IteratedGreedyPartialSolutionImprovementDelegate<TSolution,TRemoved> _improvement;

    public DelegateIteratedGreedyPartialSolutionImprovement(
        IteratedGreedyPartialSolutionImprovementDelegate<TSolution,TRemoved> improvement)
    {
        _improvement =
            improvement ??
            throw new ArgumentNullException(nameof(improvement));
    }

    public void Improve(
        ref TSolution partialSolution,
        in TRemoved removedComponents,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        CancellationToken cancellationToken) =>
        _improvement(
            ref partialSolution,
            in removedComponents,
            problem,
            random,
            cancellationToken);
}
