using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;

/// <summary>Parameters of the generic Large Neighborhood Search foundation.</summary>
public sealed class LargeNeighborhoodSearchParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Domain-defined destruction size passed unchanged to the destroy operator.
    /// </summary>
    public int DestructionSize { get; init; } = 10;

    /// <summary>Maximum number of complete destroy-repair-accept cycles.</summary>
    public int MaximumIterations { get; init; } = 500;

    public void Validate()
    {
        if (DestructionSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DestructionSize));
        }

        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }
    }
}
