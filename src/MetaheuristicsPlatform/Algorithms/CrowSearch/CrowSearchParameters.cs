using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.CrowSearch;

public sealed class CrowSearchParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 20;
    public int MaximumIterations { get; init; } = 200;
    public double FlightLength { get; init; } = 2.0;
    public double AwarenessProbability { get; init; } = 0.1;
    public void Validate()
    {
        if (PopulationSize < 2) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(FlightLength) || FlightLength <= 0.0) throw new ArgumentOutOfRangeException(nameof(FlightLength));
        if (!double.IsFinite(AwarenessProbability) || AwarenessProbability < 0.0 || AwarenessProbability > 1.0) throw new ArgumentOutOfRangeException(nameof(AwarenessProbability));
    }
}
