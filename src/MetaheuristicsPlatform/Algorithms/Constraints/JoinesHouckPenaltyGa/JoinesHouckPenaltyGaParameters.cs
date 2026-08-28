using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Constraints.JoinesHouckPenaltyGa;

public sealed class JoinesHouckPenaltyGaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 60;
    public int MaximumGenerations { get; init; } = 150;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public double PenaltyConstant { get; init; } = 0.5;
    public double Alpha { get; init; } = 2.0;
    public double Beta { get; init; } = 2.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0.0 || CrossoverProbability > 1.0) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1.0) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0.0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
        if (!double.IsFinite(PenaltyConstant) || PenaltyConstant <= 0.0) throw new ArgumentOutOfRangeException(nameof(PenaltyConstant));
        if (!double.IsFinite(Alpha) || Alpha <= 0.0) throw new ArgumentOutOfRangeException(nameof(Alpha));
        if (!double.IsFinite(Beta) || Beta <= 0.0) throw new ArgumentOutOfRangeException(nameof(Beta));
    }
}
