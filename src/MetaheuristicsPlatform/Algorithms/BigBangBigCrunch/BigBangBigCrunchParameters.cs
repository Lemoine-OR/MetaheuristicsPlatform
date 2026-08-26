using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.BigBangBigCrunch;

public sealed class BigBangBigCrunchParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;
    public double Alpha { get; init; } = 1.0;
    public void Validate()
    {
        if (PopulationSize < 2) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(Alpha) || Alpha <= 0.0) throw new ArgumentOutOfRangeException(nameof(Alpha));
    }
}
