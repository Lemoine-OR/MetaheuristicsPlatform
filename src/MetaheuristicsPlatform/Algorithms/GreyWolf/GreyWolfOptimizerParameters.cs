using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.GreyWolf;

public sealed class GreyWolfOptimizerParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;

    public void Validate()
    {
        if (PopulationSize < 3)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
    }
}
