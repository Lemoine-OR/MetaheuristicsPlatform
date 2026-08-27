using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Spea;

public sealed class SpeaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 80;
    public int ArchiveSize { get; init; } = 40;
    public int MaximumGenerations { get; init; } = 200;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;

    public void Validate()
    {
        if (PopulationSize < 4)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));

        if (ArchiveSize < 2)
            throw new ArgumentOutOfRangeException(nameof(ArchiveSize));

        if (MaximumGenerations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));

        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));

        if (!double.IsFinite(MutationProbability) ||
            MutationProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(MutationProbability));

        if (!double.IsFinite(DistributionIndex) ||
            DistributionIndex <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
    }
}
