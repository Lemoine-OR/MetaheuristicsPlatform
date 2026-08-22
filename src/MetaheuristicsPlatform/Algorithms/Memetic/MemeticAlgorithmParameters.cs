using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>Parameters of the canonical GA-backed memetic algorithm.</summary>
public sealed class MemeticAlgorithmParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Canonical generational GA parameters. The evolutionary engine is shared with
    /// <see cref="GenerationalGeneticAlgorithmOptimizer{TSolution}"/>.
    /// </summary>
    public GeneticAlgorithmParameters GeneticAlgorithm { get; init; } = new();

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(GeneticAlgorithm);
        GeneticAlgorithm.Validate();
    }
}
