using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.MipAdaptiveLns;

public sealed class MipAdaptiveLargeNeighborhoodSearchParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 16;
    public double DestroyFraction { get; init; } = 0.5;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(DestroyFraction) || DestroyFraction <= 0.0 || DestroyFraction > 1.0)
            throw new ArgumentOutOfRangeException(nameof(DestroyFraction));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
