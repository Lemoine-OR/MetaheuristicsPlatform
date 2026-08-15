namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Hash-based memory of previously visited configuration signatures.
/// </summary>
/// <remarks>
/// Battiti and Tecchiolli (1994) explicitly advocate hashing or digital-tree techniques so
/// repetition lookup remains approximately constant-time as the trajectory grows. This class
/// implements the hash-table alternative with expected O(1) observation.
/// </remarks>
public sealed class ConfigurationRepetitionMemory
{
    private readonly Dictionary<ulong, Entry> _entries;

    public ConfigurationRepetitionMemory(int initialCapacity = 256)
    {
        if (initialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        _entries = new Dictionary<ulong, Entry>(initialCapacity);
    }

    public int Count => _entries.Count;

    public TabuSearchRepetitionObservation Observe(
        ulong signature,
        long iteration)
    {
        if (iteration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iteration));
        }

        if (_entries.TryGetValue(signature, out Entry previous))
        {
            long cycleLength = iteration - previous.LastIteration;
            if (cycleLength <= 0)
            {
                throw new InvalidOperationException(
                    "A repeated configuration must be observed at a strictly later iteration.");
            }

            long visits = checked(previous.VisitCount + 1);
            _entries[signature] = new Entry(iteration, visits);

            return new TabuSearchRepetitionObservation(
                isRepetition: true,
                previousIteration: previous.LastIteration,
                cycleLength,
                visitCount: visits);
        }

        _entries.Add(signature, new Entry(iteration, 1));

        return new TabuSearchRepetitionObservation(
            isRepetition: false,
            previousIteration: -1,
            cycleLength: 0,
            visitCount: 1);
    }

    public void Clear() => _entries.Clear();

    private readonly struct Entry
    {
        public Entry(long lastIteration, long visitCount)
        {
            LastIteration = lastIteration;
            VisitCount = visitCount;
        }

        public long LastIteration { get; }
        public long VisitCount { get; }
    }
}
