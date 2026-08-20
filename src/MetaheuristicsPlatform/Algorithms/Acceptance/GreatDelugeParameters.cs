using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public sealed class GreatDelugeParameters : IMetaheuristicParameters
{
    /// <summary>Positive absolute level change after each attempted transition.</summary>
    public double RainSpeed { get; init; } = 0.01;

    public int MaximumConsecutiveSamplingFailures { get; init; } = 64;

    public void Validate()
    {
        if (!double.IsFinite(RainSpeed) || RainSpeed <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(RainSpeed));

        if (MaximumConsecutiveSamplingFailures <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumConsecutiveSamplingFailures));
    }
}