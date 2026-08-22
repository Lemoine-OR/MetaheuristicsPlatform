namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Lazy Ant System pheromone memory. Global evaporation is represented by a generation
/// counter, so evaporation is O(1) and an unseen key still receives exactly the decay
/// that tau_0 would have accumulated since iteration zero.
/// </summary>
internal sealed class AntSystemPheromoneMemory<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Entry> _entries;
    private readonly double _initialPheromone;
    private readonly double _retention;

    private int _evaporationRounds;

    public AntSystemPheromoneMemory(
        double initialPheromone,
        double evaporationRate,
        IEqualityComparer<TKey>? comparer = null)
    {
        _initialPheromone = initialPheromone;
        _retention = 1.0 - evaporationRate;
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

        double value = Decay(_initialPheromone, _evaporationRounds);
        _entries.Add(key, new Entry(value, _evaporationRounds));
        return value;
    }

    public void Evaporate() => _evaporationRounds++;

    public void Deposit(TKey key, double amount)
    {
        if (amount == 0.0)
        {
            _ = Get(key);
            return;
        }

        double current = Get(key);
        double updated = current + amount;

        if (!double.IsFinite(updated) || updated <= 0.0)
        {
            throw new InvalidOperationException(
                "Pheromone update produced a non-finite or non-positive value.");
        }

        _entries[key] = new Entry(updated, _evaporationRounds);
    }

    private Entry Materialize(Entry entry)
    {
        int rounds = _evaporationRounds - entry.LastEvaporationRound;

        if (rounds <= 0)
        {
            return entry;
        }

        return new Entry(
            Decay(entry.Value, rounds),
            _evaporationRounds);
    }

    private double Decay(double value, int rounds)
    {
        if (rounds <= 0)
        {
            return value;
        }

        double decayed =
            value * Math.Pow(_retention, rounds);

        // Numerical floor only: mathematically the classical trail remains positive
        // for 0 < rho < 1, but IEEE-754 underflow must not create log(0).
        return Math.Max(double.Epsilon, decayed);
    }

    private readonly record struct Entry(
        double Value,
        int LastEvaporationRound);
}
