using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.BiogeographyBasedOptimization;

public sealed class BiogeographyBasedOptimizationParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;
    public int EliteCount { get; init; } = 2;
    public double MaximumImmigrationRate { get; init; } = 1.0;
    public double MaximumEmigrationRate { get; init; } = 1.0;
    public double MaximumMutationRate { get; init; } = 0.01;

    public void Validate()
    {
        if (PopulationSize < 3)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (EliteCount < 0 || EliteCount >= PopulationSize)
            throw new ArgumentOutOfRangeException(nameof(EliteCount));
        if (!double.IsFinite(MaximumImmigrationRate) || MaximumImmigrationRate <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(MaximumImmigrationRate));
        if (!double.IsFinite(MaximumEmigrationRate) || MaximumEmigrationRate <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(MaximumEmigrationRate));
        if (!double.IsFinite(MaximumMutationRate) ||
            MaximumMutationRate < 0.0 ||
            MaximumMutationRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(MaximumMutationRate));
    }
}
