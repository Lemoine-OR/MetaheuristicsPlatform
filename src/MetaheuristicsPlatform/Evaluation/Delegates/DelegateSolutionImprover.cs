namespace MetaheuristicsPlatform.Evaluation.Delegates;

public sealed class DelegateSolutionImprover<TSolution> :
    ISolutionImprover<TSolution>
{
    private readonly SolutionMutationDelegate<TSolution>? _byValueImprove;
    private readonly RefSolutionMutationDelegate<TSolution>? _byRefImprove;

    /// <summary>
    /// Compatibility constructor for mutable reference-type solutions.
    /// </summary>
    public DelegateSolutionImprover(
        SolutionMutationDelegate<TSolution> improve)
    {
        ArgumentNullException.ThrowIfNull(improve);

        if (typeof(TSolution).IsValueType)
        {
            throw new ArgumentException(
                "Value-type solutions require RefSolutionMutationDelegate<TSolution> " +
                "so improved values propagate back to the pipeline.",
                nameof(improve));
        }

        _byValueImprove = improve;
    }

    /// <summary>
    /// Preferred constructor. Correct for both reference and value types.
    /// </summary>
    public DelegateSolutionImprover(
        RefSolutionMutationDelegate<TSolution> improve)
    {
        _byRefImprove =
            improve ??
            throw new ArgumentNullException(
                nameof(improve));
    }

    public bool Improve(
        ref TSolution solution,
        CancellationToken cancellationToken = default)
    {
        if (_byRefImprove is not null)
        {
            return _byRefImprove(
                ref solution,
                cancellationToken);
        }

        return _byValueImprove!(
            solution,
            cancellationToken);
    }
}