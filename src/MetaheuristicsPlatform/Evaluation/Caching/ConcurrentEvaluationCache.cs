using System.Collections.Concurrent;

namespace MetaheuristicsPlatform.Evaluation.Caching;

/// <summary>
/// Concurrent cache that suppresses duplicate evaluation work for equal keys.
/// Faulted or cancelled factories are removed and may be retried later.
/// </summary>
public sealed class ConcurrentEvaluationCache<TKey, TValue> :
    IEvaluationCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _entries;

    public ConcurrentEvaluationCache(
        IEqualityComparer<TKey>? comparer = null)
    {
        _entries =
            new ConcurrentDictionary<TKey, Lazy<TValue>>(
                comparer ??
                EqualityComparer<TKey>.Default);
    }

    public int Count =>
        _entries.Count;

    public bool TryGet(
        TKey key,
        out TValue value)
    {
        if (_entries.TryGetValue(
                key,
                out Lazy<TValue>? lazy))
        {
            try
            {
                value = lazy.Value;
                return true;
            }
            catch
            {
                RemoveIfCurrent(
                    key,
                    lazy);

                throw;
            }
        }

        value = default!;
        return false;
    }

    public EvaluationCacheLookup<TValue> GetOrAdd(
        TKey key,
        Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(
            valueFactory);

        if (_entries.TryGetValue(
                key,
                out Lazy<TValue>? existing))
        {
            return new EvaluationCacheLookup<TValue>(
                GetValueOrRemove(
                    key,
                    existing),
                IsHit: true);
        }

        var created =
            new Lazy<TValue>(
                () =>
                    valueFactory(key),
                LazyThreadSafetyMode.ExecutionAndPublication);

        Lazy<TValue> selected =
            _entries.GetOrAdd(
                key,
                created);

        bool isHit =
            !ReferenceEquals(
                selected,
                created);

        return new EvaluationCacheLookup<TValue>(
            GetValueOrRemove(
                key,
                selected),
            isHit);
    }

    public void Clear() =>
        _entries.Clear();

    private TValue GetValueOrRemove(
        TKey key,
        Lazy<TValue> lazy)
    {
        try
        {
            return lazy.Value;
        }
        catch
        {
            RemoveIfCurrent(
                key,
                lazy);

            throw;
        }
    }

    private void RemoveIfCurrent(
        TKey key,
        Lazy<TValue> lazy)
    {
        if (_entries.TryGetValue(
                key,
                out Lazy<TValue>? current) &&
            ReferenceEquals(
                current,
                lazy))
        {
            _entries.TryRemove(
                key,
                out _);
        }
    }
}