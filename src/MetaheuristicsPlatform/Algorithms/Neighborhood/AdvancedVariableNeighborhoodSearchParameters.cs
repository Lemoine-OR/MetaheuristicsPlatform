using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Parameters for Reduced Variable Neighborhood Search (RVNS).</summary>
public sealed class ReducedVariableNeighborhoodSearchParameters :
    IMetaheuristicParameters
{
    /// <summary>Maximum number of complete shaking-neighborhood cycles.</summary>
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

/// <summary>Parameters for General Variable Neighborhood Search (GVNS).</summary>
public sealed class GeneralVariableNeighborhoodSearchParameters :
    IMetaheuristicParameters
{
    /// <summary>Maximum number of complete shaking-neighborhood cycles.</summary>
    public int MaximumCycles { get; init; } = 100;

    /// <summary>Safety cap used by the VND improvement phase after each shaking move.</summary>
    public int MaximumNeighborhoodRestarts { get; init; } = 10_000;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumCycles <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumCycles));
        }

        if (MaximumNeighborhoodRestarts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumNeighborhoodRestarts));
        }
    }
}

/// <summary>Parameters for Skewed Variable Neighborhood Search (SVNS).</summary>
public sealed class SkewedVariableNeighborhoodSearchParameters :
    IMetaheuristicParameters
{
    /// <summary>Maximum number of complete shaking-neighborhood cycles.</summary>
    public int MaximumCycles { get; init; } = 100;

    /// <summary>
    /// Non-negative skewing factor multiplying the domain distance.
    /// Alpha = 0 reduces recentering to strict original-objective improvement.
    /// </summary>
    public double Alpha { get; init; } = 0.1;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumCycles <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumCycles));
        }

        if (!double.IsFinite(Alpha) || Alpha < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(Alpha));
        }
    }
}
