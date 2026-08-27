using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Paes;
public sealed class PaesParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 1000;
    public int ArchiveSize { get; init; } = 100;
    public int GridDivisions { get; init; } = 10;
    public double MutationProbability { get; init; } = -1.0;
    public double MutationDistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (ArchiveSize < 2) throw new ArgumentOutOfRangeException(nameof(ArchiveSize));
        if (GridDivisions < 2) throw new ArgumentOutOfRangeException(nameof(GridDivisions));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1.0) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(MutationDistributionIndex) || MutationDistributionIndex <= 0.0) throw new ArgumentOutOfRangeException(nameof(MutationDistributionIndex));
    }
}
