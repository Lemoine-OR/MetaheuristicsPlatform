using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.FlowerPollination;

public sealed class FlowerPollinationParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 25;
    public int MaximumIterations { get; init; } = 200;
    public double GlobalPollinationProbability { get; init; } = 0.8;
    public double LevyScale { get; init; } = 0.01;

    public void Validate()
    {
        if (PopulationSize < 3)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(GlobalPollinationProbability) ||
            GlobalPollinationProbability <= 0.0 ||
            GlobalPollinationProbability >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(GlobalPollinationProbability));
        if (!double.IsFinite(LevyScale) || LevyScale <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(LevyScale));
    }
}
