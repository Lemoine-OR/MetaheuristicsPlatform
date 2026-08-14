namespace MetaheuristicsPlatform.Evaluation.Caching;

/// <summary>
/// Produces an ownership-independent snapshot for cache storage or return.
/// </summary>
public interface IEvaluationSnapshotCloner<T>
{
    T Clone(T value);
}