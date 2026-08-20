using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

public sealed class IteratedGreedyParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Number of solution components requested from the destruction operator.
    /// The default 4 is a practical library default inspired by the classical
    /// permutation-flowshop literature, not a universal prescription.
    /// </summary>
    public int DestructionSize { get; init; } = 4;

    /// <summary>Maximum number of destroy-reconstruct-accept cycles.</summary>
    public int MaximumIterations { get; init; } = 1000;

    public void Validate()
    {
        if (DestructionSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(DestructionSize));

        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
    }
}
