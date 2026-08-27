using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.PesaII;
public sealed class PesaIIParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 60;
    public int ArchiveSize { get; init; } = 100;
    public int MaximumGenerations { get; init; } = 200;
    public int GridDivisions { get; init; } = 10;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (ArchiveSize < 2) throw new ArgumentOutOfRangeException(nameof(ArchiveSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (GridDivisions < 2) throw new ArgumentOutOfRangeException(nameof(GridDivisions));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0 || CrossoverProbability > 1) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
    }
}
