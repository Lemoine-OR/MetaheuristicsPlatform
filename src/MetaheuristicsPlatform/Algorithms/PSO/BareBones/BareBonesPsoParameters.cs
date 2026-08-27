using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.BareBones;

public sealed class BareBonesPsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 40;
    public int MaximumIterations { get; init; } = 1000;

    public void Validate()
    {
        if (SwarmSize <= 0) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
    }
}
