using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.KernelSearch;

public sealed class KernelSearchMatheuristicParameters : IMetaheuristicParameters
{
    public int KernelSize { get; init; } = 2;
    public int BucketSize { get; init; } = 2;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (KernelSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(KernelSize));
        if (BucketSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(BucketSize));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
