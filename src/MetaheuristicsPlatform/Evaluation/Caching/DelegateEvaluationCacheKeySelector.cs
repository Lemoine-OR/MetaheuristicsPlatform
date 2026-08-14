namespace MetaheuristicsPlatform.Evaluation.Caching;

public sealed class DelegateEvaluationCacheKeySelector<TCandidate, TKey> :
    IEvaluationCacheKeySelector<TCandidate, TKey>
    where TKey : notnull
{
    private readonly Func<TCandidate, TKey> _selector;

    public DelegateEvaluationCacheKeySelector(
        Func<TCandidate, TKey> selector)
    {
        _selector =
            selector ??
            throw new ArgumentNullException(
                nameof(selector));
    }

    public TKey GetKey(TCandidate candidate) =>
        _selector(candidate);
}