using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.CrowdingDe;

public sealed class CrowdingDeParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 80;
    public int MaximumGenerations { get; init; } = 180;
    public double DifferentialWeight { get; init; } = 0.5;
    public double CrossoverProbability { get; init; } = 0.9;
    public double NicheRadius { get; init; } = 0.1;
    public int MaximumOptima { get; init; } = 20;

    public void Validate()
    {
        if (PopulationSize < 4)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(DifferentialWeight) || DifferentialWeight <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(DifferentialWeight));
        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(NicheRadius) || NicheRadius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(NicheRadius));
        if (MaximumOptima <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumOptima));

    }
}
