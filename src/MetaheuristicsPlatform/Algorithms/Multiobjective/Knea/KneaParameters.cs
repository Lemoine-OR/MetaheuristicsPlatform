using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Knea;
public sealed class KneaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 100;
    public int MaximumGenerations { get; init; } = 200;
    public int KneeNeighbors { get; init; } = 5;
    public double KneePreference { get; init; } = 0.5;
    public double CrossoverProbability { get; init; } = 1.0;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (KneeNeighbors < 2) throw new ArgumentOutOfRangeException(nameof(KneeNeighbors));
        if (!double.IsFinite(KneePreference) || KneePreference < 0 || KneePreference > 1) throw new ArgumentOutOfRangeException(nameof(KneePreference));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0 || CrossoverProbability > 1) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
    }
}
