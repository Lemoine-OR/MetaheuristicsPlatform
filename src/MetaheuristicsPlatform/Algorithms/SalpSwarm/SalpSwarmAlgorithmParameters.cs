using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.SalpSwarm;
public sealed class SalpSwarmAlgorithmParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
    }
}
