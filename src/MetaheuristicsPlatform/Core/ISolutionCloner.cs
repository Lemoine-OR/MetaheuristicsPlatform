namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Creates a stable snapshot of a candidate when it becomes the best-so-far solution.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public interface ISolutionCloner<TSolution>
{
    /// <summary>Creates an independent or immutable snapshot of <paramref name="solution"/>.</summary>
    TSolution Clone(TSolution solution);
}

/// <summary>
/// Uses identity as the snapshot operation. This is safe only for immutable solution representations.
/// </summary>
/// <typeparam name="TSolution">Immutable solution representation.</typeparam>
public sealed class ImmutableSolutionCloner<TSolution> : ISolutionCloner<TSolution>
{
    /// <inheritdoc />
    public TSolution Clone(TSolution solution) => solution;
}

/// <summary>
/// Adapts a cloning delegate to <see cref="ISolutionCloner{TSolution}"/>.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public sealed class DelegateSolutionCloner<TSolution> : ISolutionCloner<TSolution>
{
    private readonly Func<TSolution, TSolution> _clone;

    /// <summary>Initializes the cloner.</summary>
    public DelegateSolutionCloner(Func<TSolution, TSolution> clone)
    {
        _clone = clone ?? throw new ArgumentNullException(nameof(clone));
    }

    /// <inheritdoc />
    public TSolution Clone(TSolution solution) => _clone(solution);
}