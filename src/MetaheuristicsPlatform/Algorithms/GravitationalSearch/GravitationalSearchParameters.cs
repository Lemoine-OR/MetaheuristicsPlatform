using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.GravitationalSearch;

public sealed class GravitationalSearchParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;
    public double InitialGravitationalConstant { get; init; } = 100.0;
    public double GravityDecay { get; init; } = 20.0;
    public double DistanceEpsilon { get; init; } = 1e-12;
    public void Validate()
    {
        if (PopulationSize < 2) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(InitialGravitationalConstant) || InitialGravitationalConstant <= 0.0) throw new ArgumentOutOfRangeException(nameof(InitialGravitationalConstant));
        if (!double.IsFinite(GravityDecay) || GravityDecay <= 0.0) throw new ArgumentOutOfRangeException(nameof(GravityDecay));
        if (!double.IsFinite(DistanceEpsilon) || DistanceEpsilon <= 0.0) throw new ArgumentOutOfRangeException(nameof(DistanceEpsilon));
    }
}
