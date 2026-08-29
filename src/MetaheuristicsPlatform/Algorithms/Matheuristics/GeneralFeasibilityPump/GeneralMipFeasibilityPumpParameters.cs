using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.GeneralFeasibilityPump;

public sealed class GeneralMipFeasibilityPumpParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 24;
    public double PerturbationFraction { get; init; } = 0.2;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(PerturbationFraction) || PerturbationFraction <= 0.0 || PerturbationFraction > 1.0)
            throw new ArgumentOutOfRangeException(nameof(PerturbationFraction));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
