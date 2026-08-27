using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.TwoArch2;
public sealed class TwoArch2Parameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 100;
    public int ConvergenceArchiveSize { get; init; } = 100;
    public int DiversityArchiveSize { get; init; } = 100;
    public int MaximumGenerations { get; init; } = 200;
    public double DiversityNormExponent { get; init; } = 0.5;
    public double CrossoverProbability { get; init; } = 1.0;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (ConvergenceArchiveSize < 2) throw new ArgumentOutOfRangeException(nameof(ConvergenceArchiveSize));
        if (DiversityArchiveSize < 2) throw new ArgumentOutOfRangeException(nameof(DiversityArchiveSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(DiversityNormExponent) || DiversityNormExponent <= 0) throw new ArgumentOutOfRangeException(nameof(DiversityNormExponent));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0 || CrossoverProbability > 1) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
    }
}
