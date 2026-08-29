using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.ImprovedFeasibilityPump;

public sealed class ImprovedFeasibilityPumpParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 20;
    public double ObjectiveWeight { get; init; } = 0.15;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(ObjectiveWeight) || ObjectiveWeight < 0.0 || ObjectiveWeight > 1.0)
            throw new ArgumentOutOfRangeException(nameof(ObjectiveWeight));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
