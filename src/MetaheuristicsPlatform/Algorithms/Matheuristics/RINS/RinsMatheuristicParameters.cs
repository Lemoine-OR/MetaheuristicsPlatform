using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.RINS;

public sealed class RinsMatheuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 4;
    public double AgreementTolerance { get; init; } = 1e-6;
    public int NodeLimit { get; init; } = 1000;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(AgreementTolerance) || AgreementTolerance < 0.0)
            throw new ArgumentOutOfRangeException(nameof(AgreementTolerance));
        if (NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(NodeLimit));
    }
}
