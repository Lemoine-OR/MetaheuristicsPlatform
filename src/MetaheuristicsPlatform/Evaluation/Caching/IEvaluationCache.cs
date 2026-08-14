namespace MetaheuristicsPlatform.Evaluation.Caching;

/// <summary>
/// Thread-safe cache contract for evaluation outcomes.
/// </summary>
public interface IEvaluationCache<TKey, TValue>
    where TKey : notnull
{
    int Count { get; }

    bool TryGet(
        TKey key,
        out TValue value);

    EvaluationCacheLookup<TValue> GetOrAdd(
        TKey key,
        Func<TKey, TValue> valueFactory);

    void Clear();
}