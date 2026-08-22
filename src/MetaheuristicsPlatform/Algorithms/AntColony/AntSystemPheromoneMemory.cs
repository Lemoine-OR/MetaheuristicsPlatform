namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Sparse lazy pheromone memory shared by Ant System descendants.
/// Optional lower/upper bounds make the same storage usable by MMAS.
/// </summary>
internal sealed class AntSystemPheromoneMemory<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Entry> _entries;
    private readonly double _initialPheromone;
    private readonly double _retention;
    private readonly double _minimum;
    private readonly double _maximum;

    private int _evaporationRounds;

    public AntSystemPheromoneMemory(
        double initialPheromone,
        double evaporationRate,
        IEqualityComparer<TKey>? comparer = null,
        double minimum = double.Epsilon,
        double maximum = double.PositiveInfinity)
    {
        if (!double.IsFinite(initialPheromone) ||
            initialPheromone <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialPheromone));
        }

        if (!double.IsFinite(evaporationRate) ||
            evaporationRate < 0.0 ||
            evaporationRate >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(evaporationRate));
        }

        if (!double.IsFinite(minimum) ||
            minimum <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimum));
        }

        if (double.IsNaN(maximum) ||
            maximum < minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum));
        }

        _initialPheromone = Clamp(initialPheromone, minimum, maximum);
        _retention = 1.0 - evaporationRate;
        _minimum = minimum;
        _maximum = maximum;
        _entries = new Dictionary<TKey, Entry>(comparer);
    }

    public int Count => _entries.Count;

    public int EvaporationRounds => _evaporationRounds;

    public double Get(TKey key)
    {
        if (_entries.TryGetValue(key, out Entry entry))
        {
            entry = Materialize(entry);
            _entries[key] = entry;
            return entry.Value;
        }

        double value =
            Bound(Decay(_initialPheromone, _evaporationRounds));

        _entries.Add(
            key,
            new Entry(value, _evaporationRounds));

        return value;
    }

    public void Evaporate() =>
        _evaporationRounds++;

    public void Deposit(TKey key, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        double current = Get(key);
        Set(key, current + amount);
    }

    public void Set(TKey key, double value)
    {
        if (!double.IsFinite(value) || value <= 0.0)
        {
            throw new InvalidOperationException(
                "Pheromone update produced a non-finite or non-positive value.");
        }

        _entries[key] =
            new Entry(
                Bound(value),
                _evaporationRounds);
    }

    public void Reset()
    {
        _entries.Clear();
        _evaporationRounds = 0;
    }

    private Entry Materialize(Entry entry)
    {
        int rounds =
            _evaporationRounds -
            entry.LastEvaporationRound;

        if (rounds <= 0)
        {
            return entry;
        }

        return new Entry(
            Bound(Decay(entry.Value, rounds)),
            _evaporationRounds);
    }

    private double Decay(double value, int rounds)
    {
        if (rounds <= 0)
        {
            return value;
        }

        return value *
            Math.Pow(_retention, rounds);
    }

    private double Bound(double value) =>
        Clamp(
            Math.Max(double.Epsilon, value),
            _minimum,
            _maximum);

    private static double Clamp(
        double value,
        double minimum,
        double maximum) =>
        Math.Min(
            maximum,
            Math.Max(minimum, value));

    private readonly record struct Entry(
        double Value,
        int LastEvaporationRound);
}
