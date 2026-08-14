namespace MetaheuristicsPlatform.Evaluation.Caching;

public interface IEvaluationCacheKeySelector<in TCandidate, out TKey>
    where TKey : notnull
{
    TKey GetKey(TCandidate candidate);
}