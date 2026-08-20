using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public sealed class LateAcceptanceParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Number of objective values retained in the circular late-acceptance history.
    /// The default is a library convenience, not a universal value prescribed by the paper.
    /// </summary>
    public int HistoryLength { get; init; } = 100;

    public int MaximumConsecutiveSamplingFailures { get; init; } = 64;

    public void Validate()
    {
        if (HistoryLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(HistoryLength));

        if (MaximumConsecutiveSamplingFailures <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumConsecutiveSamplingFailures));
    }
}
