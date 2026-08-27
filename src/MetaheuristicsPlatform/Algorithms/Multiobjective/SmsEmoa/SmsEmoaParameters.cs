using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.SmsEmoa;
public sealed class SmsEmoaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 80;
    public int MaximumIterations { get; init; } = 1000;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0 || CrossoverProbability > 1) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
    }
}
