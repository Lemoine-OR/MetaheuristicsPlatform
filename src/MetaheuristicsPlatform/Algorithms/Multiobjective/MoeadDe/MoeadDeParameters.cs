using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.MoeadDe;
public sealed class MoeadDeParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 100;
    public int MaximumGenerations { get; init; } = 200;
    public int NeighborhoodSize { get; init; } = 20;
    public double NeighborhoodMatingProbability { get; init; } = 0.9;
    public double DifferentialWeight { get; init; } = 0.5;
    public double CrossoverProbability { get; init; } = 1.0;
    public int MaximumReplacements { get; init; } = 2;
    public void Validate()
    {
        if (PopulationSize < 5) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (NeighborhoodSize < 3 || NeighborhoodSize > PopulationSize) throw new ArgumentOutOfRangeException(nameof(NeighborhoodSize));
        if (!double.IsFinite(NeighborhoodMatingProbability) || NeighborhoodMatingProbability < 0 || NeighborhoodMatingProbability > 1) throw new ArgumentOutOfRangeException(nameof(NeighborhoodMatingProbability));
        if (!double.IsFinite(DifferentialWeight) || DifferentialWeight <= 0 || DifferentialWeight > 2) throw new ArgumentOutOfRangeException(nameof(DifferentialWeight));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0 || CrossoverProbability > 1) throw new ArgumentOutOfRangeException(nameof(CrossoverProbability));
        if (MaximumReplacements < 1) throw new ArgumentOutOfRangeException(nameof(MaximumReplacements));
    }
}
