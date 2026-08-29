using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.RENS;

public sealed class RensMatheuristicParameters : IMetaheuristicParameters
{
    public double IntegralityTolerance { get; init; } = 1e-6;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (!double.IsFinite(IntegralityTolerance) || IntegralityTolerance < 0.0)
            throw new ArgumentOutOfRangeException(nameof(IntegralityTolerance));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
