using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.LocalBranching;

public sealed class LocalBranchingMatheuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 8;
    public int HammingRadius { get; init; } = 2;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (HammingRadius <= 0)
            throw new ArgumentOutOfRangeException(nameof(HammingRadius));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
