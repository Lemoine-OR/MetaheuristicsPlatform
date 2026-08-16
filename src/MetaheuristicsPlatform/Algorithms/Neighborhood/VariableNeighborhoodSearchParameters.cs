using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Parameters for standalone Variable Neighborhood Descent.</summary>
public sealed class VariableNeighborhoodDescentParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Safety cap on the number of times VND may restart from the first neighborhood
    /// after a strict improvement.
    /// </summary>
    public int MaximumNeighborhoodRestarts { get; init; } = 10_000;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumNeighborhoodRestarts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumNeighborhoodRestarts));
        }
    }
}

/// <summary>Parameters for canonical basic Variable Neighborhood Search.</summary>
public sealed class VariableNeighborhoodSearchParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Maximum number of complete VNS cycles. Each cycle scans the ordered shaking
    /// neighborhoods and restarts at the first neighborhood after every strict improvement.
    /// </summary>
    public int MaximumCycles { get; init; } = 100;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumCycles <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumCycles));
        }
    }
}
