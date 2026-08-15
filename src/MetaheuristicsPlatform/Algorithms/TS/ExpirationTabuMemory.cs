namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Expiration-based attribute memory with ordered removal of expired entries.
/// </summary>
/// <remarks>
/// The dictionary provides expected constant-time tabu lookup. A min-priority queue orders
/// expiration records by iteration, so varying tabu tenures are cleaned correctly even when
/// later registrations expire before earlier ones. Re-registering an attribute is safe:
/// stale priority-queue records are ignored when their expiration no longer matches the
/// dictionary.
/// </remarks>
public sealed class ExpirationTabuMemory<TAttribute> :
    ITabuMemory<TAttribute>
    where TAttribute : notnull
{
    private readonly Dictionary<TAttribute, long> _tabuUntil;
    private readonly PriorityQueue<ExpirationEntry, long> _expirations;

    public ExpirationTabuMemory(
        int initialCapacity = 128,
        IEqualityComparer<TAttribute>? comparer = null)
    {
        if (initialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        _tabuUntil = new Dictionary<TAttribute, long>(initialCapacity, comparer);
        _expirations = new PriorityQueue<ExpirationEntry, long>(initialCapacity);
    }

    public int Count => _tabuUntil.Count;

    public void Advance(long iteration)
    {
        while (_expirations.TryPeek(
                   out ExpirationEntry entry,
                   out long expiration) &&
               expiration < iteration)
        {
            _expirations.Dequeue();

            if (_tabuUntil.TryGetValue(entry.Attribute, out long current) &&
                current == entry.TabuUntilIteration)
            {
                _tabuUntil.Remove(entry.Attribute);
            }
        }
    }

    public bool IsTabu(
        in TAttribute attribute,
        long iteration)
    {
        if (!_tabuUntil.TryGetValue(attribute, out long tabuUntilIteration))
        {
            return false;
        }

        if (tabuUntilIteration >= iteration)
        {
            return true;
        }

        _tabuUntil.Remove(attribute);
        return false;
    }

    public void Register(
        in TAttribute attribute,
        long tabuUntilIteration)
    {
        if (tabuUntilIteration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tabuUntilIteration));
        }

        _tabuUntil[attribute] = tabuUntilIteration;
        _expirations.Enqueue(
            new ExpirationEntry(attribute, tabuUntilIteration),
            tabuUntilIteration);
    }

    private readonly struct ExpirationEntry
    {
        public ExpirationEntry(
            TAttribute attribute,
            long tabuUntilIteration)
        {
            Attribute = attribute;
            TabuUntilIteration = tabuUntilIteration;
        }

        public TAttribute Attribute { get; }
        public long TabuUntilIteration { get; }
    }
}
