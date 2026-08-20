using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public sealed class DemonAcceptanceParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Initial non-negative Demon credit/energy. The default is a library convenience;
    /// the meaningful scale is problem dependent.
    /// </summary>
    public double InitialCredit { get; init; } = 1.0;

    public int MaximumConsecutiveSamplingFailures { get; init; } = 64;

    public void Validate()
    {
        if (!double.IsFinite(InitialCredit) || InitialCredit < 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialCredit));

        if (MaximumConsecutiveSamplingFailures <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumConsecutiveSamplingFailures));
    }
}
