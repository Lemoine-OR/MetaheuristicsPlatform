namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Creates independent snapshots of one-dimensional array solutions.
/// </summary>
/// <typeparam name="T">Array element type.</typeparam>
public sealed class ArraySolutionCloner<T> : ISolutionCloner<T[]>
{
    /// <inheritdoc />
    public T[] Clone(T[] solution)
    {
        ArgumentNullException.ThrowIfNull(solution);
        return (T[])solution.Clone();
    }
}