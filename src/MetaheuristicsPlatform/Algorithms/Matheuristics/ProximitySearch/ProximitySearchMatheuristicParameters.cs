using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.ProximitySearch;

public sealed class ProximitySearchMatheuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 8;
    public double MinimumImprovement { get; init; } = 1e-6;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(MinimumImprovement) || MinimumImprovement <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(MinimumImprovement));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
