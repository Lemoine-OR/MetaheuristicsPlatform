using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Rvea;
public sealed class RveaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 100;
    public int MaximumGenerations { get; init; } = 200;
    public int ReferenceDivisions { get; init; } = 12;
    public double Alpha { get; init; } = 2.0;
    public int AdaptationFrequency { get; init; } = 20;
    public double CrossoverProbability { get; init; } = 1.0;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (ReferenceDivisions < 1) throw new ArgumentOutOfRangeException(nameof(ReferenceDivisions));
        if (!double.IsFinite(Alpha) || Alpha <= 0) throw new ArgumentOutOfRangeException(nameof(Alpha));
        if (AdaptationFrequency <= 0) throw new ArgumentOutOfRangeException(nameof(AdaptationFrequency));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0 || CrossoverProbability > 1) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
    }
}
