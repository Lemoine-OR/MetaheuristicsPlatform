namespace MetaheuristicsPlatform.Evaluation.Delegates;

public sealed class DelegateLamarckianFeedback<TCandidate, TSolution> :
    ILamarckianFeedback<TCandidate, TSolution>
{
    private readonly LamarckianFeedbackDelegate<TCandidate, TSolution> _apply;

    public DelegateLamarckianFeedback(
        LamarckianFeedbackDelegate<TCandidate, TSolution> apply)
    {
        _apply =
            apply ??
            throw new ArgumentNullException(
                nameof(apply));
    }

    public void Apply(
        TSolution improvedSolution,
        ref TCandidate candidate,
        CancellationToken cancellationToken = default) =>
        _apply(
            improvedSolution,
            ref candidate,
            cancellationToken);
}