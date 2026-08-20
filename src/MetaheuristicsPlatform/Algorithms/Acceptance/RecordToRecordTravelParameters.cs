using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public sealed class RecordToRecordTravelParameters : IMetaheuristicParameters
{
    public double Deviation { get; init; } = 1.0;
    public int MaximumConsecutiveSamplingFailures { get; init; } = 64;

    public void Validate()
    {
        if (!double.IsFinite(Deviation) || Deviation < 0.0)
            throw new ArgumentOutOfRangeException(nameof(Deviation));

        if (MaximumConsecutiveSamplingFailures <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumConsecutiveSamplingFailures));
    }
}