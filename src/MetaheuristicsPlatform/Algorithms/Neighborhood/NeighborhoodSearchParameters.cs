using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Parameters shared by first- and best-improvement local search.</summary>
public sealed class LocalSearchParameters : IMetaheuristicParameters
{
    /// <summary>Maximum number of improving moves accepted by the standalone descent.</summary>
    public int MaximumAcceptedMoves { get; init; } = int.MaxValue;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumAcceptedMoves <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumAcceptedMoves));
        }
    }
}

/// <summary>Parameters for sequential multi-start local search.</summary>
public sealed class MultiStartLocalSearchParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Maximum number of independently generated starts. This is an algorithmic safety cap;
    /// problem-dependent stopping criteria remain available through the common platform lifecycle.
    /// </summary>
    public int MaximumStarts { get; init; } = 32;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumStarts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumStarts));
        }
    }
}

/// <summary>Parameters for the Lourenço-Martin-Stützle iterated-local-search framework.</summary>
public sealed class IteratedLocalSearchParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Maximum number of perturbation/local-search cycles after the initial descent.
    /// </summary>
    public int MaximumIterations { get; init; } = 100;

    /// <summary>Built-in incumbent acceptance rule.</summary>
    public NeighborhoodAcceptanceKind Acceptance { get; init; } =
        NeighborhoodAcceptanceKind.ImprovingOnly;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }

        if (!Enum.IsDefined(Acceptance))
        {
            throw new ArgumentOutOfRangeException(nameof(Acceptance));
        }
    }
}
