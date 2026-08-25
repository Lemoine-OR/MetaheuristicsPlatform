using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.CuckooSearch;

public sealed class CuckooSearchParameters : IMetaheuristicParameters
{
    public int NestCount { get; init; } = 25;
    public int MaximumIterations { get; init; } = 200;
    public double DiscoveryProbability { get; init; } = 0.25;
    public double LevyScale { get; init; } = 1.0;

    public void Validate()
    {
        if (NestCount < 3)
            throw new ArgumentOutOfRangeException(nameof(NestCount));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(DiscoveryProbability) ||
            DiscoveryProbability <= 0.0 ||
            DiscoveryProbability >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(DiscoveryProbability));
        if (!double.IsFinite(LevyScale) || LevyScale <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(LevyScale));
    }
}
