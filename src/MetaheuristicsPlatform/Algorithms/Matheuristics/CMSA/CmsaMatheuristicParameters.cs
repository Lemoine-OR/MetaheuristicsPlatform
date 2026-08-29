using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.CMSA;

public sealed class CmsaMatheuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 12;
    public int ConstructionsPerIteration { get; init; } = 4;
    public int MaximumAge { get; init; } = 4;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (ConstructionsPerIteration <= 0)
            throw new ArgumentOutOfRangeException(nameof(ConstructionsPerIteration));
        if (MaximumAge <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumAge));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
