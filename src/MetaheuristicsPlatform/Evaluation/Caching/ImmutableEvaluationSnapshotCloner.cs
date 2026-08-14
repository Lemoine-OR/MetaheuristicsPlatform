namespace MetaheuristicsPlatform.Evaluation.Caching;

/// <summary>
/// Identity cloner for values that are genuinely immutable.
/// Do not use for mutable reference types.
/// </summary>
public sealed class ImmutableEvaluationSnapshotCloner<T> :
    IEvaluationSnapshotCloner<T>
{
    public T Clone(T value) =>
        value;
}