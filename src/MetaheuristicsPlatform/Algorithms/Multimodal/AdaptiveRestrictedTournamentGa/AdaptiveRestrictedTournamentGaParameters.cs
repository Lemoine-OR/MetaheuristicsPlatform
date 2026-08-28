using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.AdaptiveRestrictedTournamentGa;

public sealed class AdaptiveRestrictedTournamentGaParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 80;
    public int MaximumGenerations { get; init; } = 160;
    public double CrossoverProbability { get; init; } = 0.9;
    public double MutationProbability { get; init; } = -1.0;
    public double DistributionIndex { get; init; } = 20.0;
    public double NicheRadius { get; init; } = 0.1;
    public int MaximumOptima { get; init; } = 20;
    public int MinimumTournamentWindow { get; init; } = 3;
    public void Validate()
    {
        if (PopulationSize < 4)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (!double.IsFinite(MutationProbability) ||
            MutationProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(MutationProbability));
        if (!double.IsFinite(DistributionIndex) ||
            DistributionIndex <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(DistributionIndex));
        if (!double.IsFinite(NicheRadius) || NicheRadius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(NicheRadius));
        if (MaximumOptima <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumOptima));
        if (MinimumTournamentWindow < 2)
            throw new ArgumentOutOfRangeException(nameof(MinimumTournamentWindow));
    }
}
