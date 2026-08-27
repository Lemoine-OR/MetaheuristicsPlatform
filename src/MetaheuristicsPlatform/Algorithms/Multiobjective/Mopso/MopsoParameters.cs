using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Mopso;
public sealed class MopsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 100;
    public int ArchiveSize { get; init; } = 100;
    public int MaximumIterations { get; init; } = 100;
    public int GridDivisions { get; init; } = 30;
    public double InertiaWeight { get; init; } = 0.4;
    public double MutationRate { get; init; } = 0.5;
    public void Validate()
    {
        if (SwarmSize < 4) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (ArchiveSize < 2) throw new ArgumentOutOfRangeException(nameof(ArchiveSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (GridDivisions < 2) throw new ArgumentOutOfRangeException(nameof(GridDivisions));
        if (!double.IsFinite(InertiaWeight) || InertiaWeight < 0) throw new ArgumentOutOfRangeException(nameof(InertiaWeight));
        if (!double.IsFinite(MutationRate) || MutationRate <= 0) throw new ArgumentOutOfRangeException(nameof(MutationRate));
    }
}
