using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.KernelPump;

public sealed class KernelPumpMatheuristicParameters : IMetaheuristicParameters
{
    public int BucketCount { get; init; } = 3;
    public int MaximumPumpIterations { get; init; } = 8;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (BucketCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(BucketCount));
        if (MaximumPumpIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumPumpIterations));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
