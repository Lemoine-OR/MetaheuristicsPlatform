namespace MetaheuristicsPlatform.Evaluation.Delegates;

public sealed class DelegateSolutionDecoder<TCandidate, TSolution> :
    ISolutionDecoder<TCandidate, TSolution>
{
    private readonly SolutionDecoderDelegate<TCandidate, TSolution> _decode;

    public DelegateSolutionDecoder(
        SolutionDecoderDelegate<TCandidate, TSolution> decode)
    {
        _decode =
            decode ??
            throw new ArgumentNullException(
                nameof(decode));
    }

    public TSolution Decode(
        TCandidate candidate,
        CancellationToken cancellationToken = default) =>
        _decode(
            candidate,
            cancellationToken);
}