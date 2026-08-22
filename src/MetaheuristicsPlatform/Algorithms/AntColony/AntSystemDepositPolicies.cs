using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Representation-independent constant deposit policy.
/// Useful when the domain supplies quality information outside the raw objective scale.
/// </summary>
public sealed class ConstantAntSystemDepositPolicy<TSolution> :
    IAntSystemDepositPolicy<TSolution>
{
    private readonly double _deposit;

    public ConstantAntSystemDepositPolicy(double deposit = 1.0)
    {
        if (!double.IsFinite(deposit) || deposit < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(deposit));
        }

        _deposit = deposit;
    }

    public string Id => AntColonyComponentIds.ConstantDeposit;

    public double GetDeposit(
        in TSolution solution,
        double objective,
        int antIndex,
        int antCount,
        IOptimizationProblem<TSolution> problem) =>
        _deposit;
}

/// <summary>
/// Classical Ant-System-style Q/L deposit for positive minimization objectives.
/// The explicit domain restriction prevents a misleading generic inversion of arbitrary objectives.
/// </summary>
public sealed class PositiveInverseObjectiveAntSystemDepositPolicy<TSolution> :
    IAntSystemDepositPolicy<TSolution>
{
    private readonly double _q;

    public PositiveInverseObjectiveAntSystemDepositPolicy(double q = 1.0)
    {
        if (!double.IsFinite(q) || q <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(q));
        }

        _q = q;
    }

    public string Id => AntColonyComponentIds.PositiveInverseObjectiveDeposit;

    public double GetDeposit(
        in TSolution solution,
        double objective,
        int antIndex,
        int antCount,
        IOptimizationProblem<TSolution> problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (problem.Sense != OptimizationSense.Minimize)
        {
            throw new InvalidOperationException(
                "The Q/L Ant System deposit is defined here only for minimization problems.");
        }

        if (!double.IsFinite(objective) || objective <= 0.0)
        {
            throw new InvalidOperationException(
                "The Q/L Ant System deposit requires a finite strictly-positive objective.");
        }

        return _q / objective;
    }
}
