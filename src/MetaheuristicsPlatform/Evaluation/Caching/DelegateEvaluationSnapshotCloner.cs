namespace MetaheuristicsPlatform.Evaluation.Caching;

public sealed class DelegateEvaluationSnapshotCloner<T> :
    IEvaluationSnapshotCloner<T>
{
    private readonly Func<T, T> _clone;

    public DelegateEvaluationSnapshotCloner(
        Func<T, T> clone)
    {
        _clone =
            clone ??
            throw new ArgumentNullException(
                nameof(clone));
    }

    public T Clone(T value) =>
        _clone(value);
}