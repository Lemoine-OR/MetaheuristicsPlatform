using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Smpso;
public sealed class SmpsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 100;
    public int ArchiveSize { get; init; } = 100;
    public int MaximumIterations { get; init; } = 250;
    public double InertiaWeight { get; init; } = 0.2;
    public double MinAcceleration { get; init; } = 1.0;
    public double MaxAcceleration { get; init; } = 2.5;
    public double MutationDistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (SwarmSize < 4) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (ArchiveSize < 2) throw new ArgumentOutOfRangeException(nameof(ArchiveSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(InertiaWeight) || InertiaWeight < 0) throw new ArgumentOutOfRangeException(nameof(InertiaWeight));
        if (!double.IsFinite(MinAcceleration) || !double.IsFinite(MaxAcceleration) ||
            MinAcceleration <= 0 || MaxAcceleration < MinAcceleration) throw new ArgumentOutOfRangeException(nameof(MinAcceleration));
        if (!double.IsFinite(MutationDistributionIndex) || MutationDistributionIndex <= 0) throw new ArgumentOutOfRangeException(nameof(MutationDistributionIndex));
    }
}
