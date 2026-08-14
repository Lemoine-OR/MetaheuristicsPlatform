namespace MetaheuristicsPlatform.Evaluation.Delegates;

public sealed class DelegateSolutionRepair<TSolution> :
    ISolutionRepair<TSolution>
{
    private readonly SolutionMutationDelegate<TSolution>? _byValueRepair;
    private readonly RefSolutionMutationDelegate<TSolution>? _byRefRepair;

    /// <summary>
    /// Compatibility constructor for mutable reference-type solutions.
    /// </summary>
    public DelegateSolutionRepair(
        SolutionMutationDelegate<TSolution> repair)
    {
        ArgumentNullException.ThrowIfNull(repair);

        if (typeof(TSolution).IsValueType)
        {
            throw new ArgumentException(
                "Value-type solutions require RefSolutionMutationDelegate<TSolution> " +
                "so repaired values propagate back to the pipeline.",
                nameof(repair));
        }

        _byValueRepair = repair;
    }

    /// <summary>
    /// Preferred constructor. Correct for both reference and value types.
    /// </summary>
    public DelegateSolutionRepair(
        RefSolutionMutationDelegate<TSolution> repair)
    {
        _byRefRepair =
            repair ??
            throw new ArgumentNullException(
                nameof(repair));
    }

    public bool Repair(
        ref TSolution solution,
        CancellationToken cancellationToken = default)
    {
        if (_byRefRepair is not null)
        {
            return _byRefRepair(
                ref solution,
                cancellationToken);
        }

        return _byValueRepair!(
            solution,
            cancellationToken);
    }
}