using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Constraints.HomaifarPenaltyGa;

public sealed class HomaifarPenaltyGaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 60;
    public int MaximumGenerations { get; init; } = 150;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public IReadOnlyList<IReadOnlyList<double>> ViolationLevelUpperBounds { get; init; } = Array.Empty<IReadOnlyList<double>>();
    public IReadOnlyList<IReadOnlyList<double>> PenaltyCoefficients { get; init; } = Array.Empty<IReadOnlyList<double>>();
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0.0 || CrossoverProbability > 1.0) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) || MutationProbability > 1.0) throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) || DistributionIndex <= 0.0) throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
        if (ViolationLevelUpperBounds.Count == 0 || PenaltyCoefficients.Count != ViolationLevelUpperBounds.Count) throw new ArgumentException("One level/coefficient vector is required per constrained component.");
        for(int j=0;j<ViolationLevelUpperBounds.Count;j++){var bounds=ViolationLevelUpperBounds[j];var coefficients=PenaltyCoefficients[j];if(bounds.Count==0||coefficients.Count!=bounds.Count)throw new ArgumentException("Each constrained component requires aligned level and coefficient vectors.");double previous=0.0;for(int i=0;i<bounds.Count;i++){if(!double.IsFinite(bounds[i])||bounds[i]<=previous)throw new ArgumentException("Violation level bounds must be finite and strictly increasing.");if(!double.IsFinite(coefficients[i])||coefficients[i]<0.0)throw new ArgumentException("Penalty coefficients must be finite and nonnegative.");previous=bounds[i];}}
    }
}
