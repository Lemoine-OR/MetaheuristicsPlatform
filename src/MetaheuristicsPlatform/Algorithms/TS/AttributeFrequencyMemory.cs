namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Long-term frequency memory for domain-defined tabu attributes.
/// </summary>
/// <remarks>
/// Frequency memory is one of the classical longer-term memory mechanisms described by
/// Glover for diversification and strategic bias. The memory stores only visit counts; the
/// optimizer decides how those counts influence candidate ranking.
/// </remarks>
public sealed class AttributeFrequencyMemory<TAttribute>
    where TAttribute : notnull
{
    private readonly Dictionary<TAttribute, long> _frequency;

    public AttributeFrequencyMemory(
        int initialCapacity = 128,
        IEqualityComparer<TAttribute>? comparer = null)
    {
        if (initialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        _frequency = new Dictionary<TAttribute, long>(
            initialCapacity,
            comparer);
    }

    public int Count => _frequency.Count;

    public long GetFrequency(in TAttribute attribute) =>
        _frequency.TryGetValue(attribute, out long count)
            ? count
            : 0L;

    public long Record(in TAttribute attribute)
    {
        long next = checked(GetFrequency(in attribute) + 1L);
        _frequency[attribute] = next;
        return next;
    }

    public void Clear() => _frequency.Clear();
}
