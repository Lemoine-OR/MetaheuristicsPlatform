using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public sealed class FullyInformedPsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 40;
    public int MaximumIterations { get; init; } = 1000;
    public double TotalAccelerationCoefficient { get; init; } = 4.10;
    public double Kappa { get; init; } = 1.0;
    public double InitialVelocityRangeFraction { get; init; } = 0.5;

    public void Validate()
    {
        if (SwarmSize <= 1) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(TotalAccelerationCoefficient) || TotalAccelerationCoefficient <= 4.0)
            throw new ArgumentOutOfRangeException(nameof(TotalAccelerationCoefficient));
        if (!double.IsFinite(Kappa) || Kappa <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(Kappa));
        if (!double.IsFinite(InitialVelocityRangeFraction) || InitialVelocityRangeFraction < 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialVelocityRangeFraction));
    }
}
