using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaII;

public sealed class NsgaIIParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 80;
    public int MaximumGenerations { get; init; } = 200;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double CrossoverDistributionIndex { get; init; } = 20.0;
    public double MutationDistributionIndex { get; init; } = 20.0;

    public void Validate()
    {
        if (PopulationSize < 4)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) ||
            MutationProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(CrossoverDistributionIndex) ||
            CrossoverDistributionIndex <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(CrossoverDistributionIndex));
        if (!double.IsFinite(MutationDistributionIndex) ||
            MutationDistributionIndex <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(MutationDistributionIndex));
    }
}
