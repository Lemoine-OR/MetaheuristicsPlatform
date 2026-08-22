using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Parameters of the canonical Ant System foundation.</summary>
public sealed class AntSystemParameters : IMetaheuristicParameters
{
    /// <summary>Number of ants constructing complete solutions per full iteration.</summary>
    public int AntCount { get; init; } = 20;

    /// <summary>Maximum number of full Ant System iterations.</summary>
    public int MaximumIterations { get; init; } = 100;

    /// <summary>Pheromone exponent alpha.</summary>
    public double Alpha { get; init; } = 1.0;

    /// <summary>Heuristic-information exponent beta.</summary>
    public double Beta { get; init; } = 2.0;

    /// <summary>Global evaporation coefficient rho in (0,1).</summary>
    public double EvaporationRate { get; init; } = 0.5;

    /// <summary>Initial pheromone value tau_0 for every lazily encountered key.</summary>
    public double InitialPheromone { get; init; } = 1.0;

    /// <summary>Safety bound on construction decisions performed by one ant.</summary>
    public int MaximumConstructionSteps { get; init; } = 100000;

    public void Validate()
    {
        if (AntCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(AntCount));
        }

        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }

        if (!double.IsFinite(Alpha) || Alpha < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(Alpha));
        }

        if (!double.IsFinite(Beta) || Beta < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(Beta));
        }

        if (!double.IsFinite(EvaporationRate) ||
            EvaporationRate <= 0.0 ||
            EvaporationRate >= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(EvaporationRate),
                "EvaporationRate must belong to the open interval (0,1).");
        }

        if (!double.IsFinite(InitialPheromone) ||
            InitialPheromone <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialPheromone));
        }

        if (MaximumConstructionSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumConstructionSteps));
        }
    }
}
