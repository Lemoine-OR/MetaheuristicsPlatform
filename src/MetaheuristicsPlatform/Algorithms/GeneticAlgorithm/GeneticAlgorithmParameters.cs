using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Parameters for the fixed-size generational Genetic Algorithm foundation.
/// </summary>
public sealed class GeneticAlgorithmParameters : IMetaheuristicParameters
{
    /// <summary>Number of evaluated members maintained in each complete population.</summary>
    public int PopulationSize { get; init; } = 100;

    /// <summary>Maximum number of completed offspring generations.</summary>
    public int MaximumGenerations { get; init; } = 100;

    /// <summary>
    /// Probability that one selected parent pair is passed to the configured crossover
    /// method. When crossover is skipped, independent parent clones become raw offspring.
    /// </summary>
    public double CrossoverProbability { get; init; } = 0.9;

    /// <summary>
    /// Probability that each raw offspring is passed once to the configured mutation
    /// method. Representation-specific per-locus probabilities belong inside that method.
    /// </summary>
    public double MutationProbability { get; init; } = 1.0;

    /// <summary>
    /// Number of best current members copied unchanged into the next population.
    /// Zero gives pure generational replacement without elitism.
    /// </summary>
    public int EliteCount { get; init; } = 0;

    public void Validate()
    {
        if (PopulationSize < 2)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));

        if (MaximumGenerations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));

        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CrossoverProbability));
        }

        if (!double.IsFinite(MutationProbability) ||
            MutationProbability < 0.0 ||
            MutationProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MutationProbability));
        }

        if (EliteCount < 0 ||
            EliteCount >= PopulationSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(EliteCount),
                "EliteCount must be non-negative and strictly smaller than PopulationSize.");
        }
    }
}
