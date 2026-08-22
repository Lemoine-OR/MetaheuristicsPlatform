using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.ArtificialBeeColony;

/// <summary>Parameters of the canonical continuous Artificial Bee Colony algorithm.</summary>
public sealed class ArtificialBeeColonyParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Number of food sources. The employed-bee count and the onlooker-bee count
    /// both equal this value.
    /// </summary>
    public int FoodSourceCount { get; init; } = 20;

    /// <summary>
    /// Number of complete ABC cycles before the local algorithm limit stops the run.
    /// The supplied platform stopping criterion remains globally authoritative.
    /// </summary>
    public int MaximumCycles { get; init; } = 1000;

    /// <summary>
    /// Number of consecutive unsuccessful trials after which a food source is
    /// abandoned. Zero selects FoodSourceCount * dimension.
    /// </summary>
    public int AbandonmentLimit { get; init; }

    public void Validate()
    {
        if (FoodSourceCount < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FoodSourceCount),
                "Artificial Bee Colony requires at least two food sources.");
        }

        if (MaximumCycles <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumCycles));
        }

        if (AbandonmentLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(AbandonmentLimit));
        }
    }
}
