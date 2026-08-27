using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.ComprehensiveLearning;

public sealed class ComprehensiveLearningPsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 40;
    public int MaximumIterations { get; init; } = 1000;
    public double InitialInertiaWeight { get; init; } = 0.9;
    public double FinalInertiaWeight { get; init; } = 0.4;
    public double AccelerationCoefficient { get; init; } = 1.49445;
    public int RefreshingGap { get; init; } = 7;
    public double InitialVelocityRangeFraction { get; init; } = 0.2;

    public void Validate()
    {
        if (SwarmSize < 3) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        RequirePositive(InitialInertiaWeight, nameof(InitialInertiaWeight));
        RequirePositive(FinalInertiaWeight, nameof(FinalInertiaWeight));
        RequirePositive(AccelerationCoefficient, nameof(AccelerationCoefficient));
        if (RefreshingGap <= 0) throw new ArgumentOutOfRangeException(nameof(RefreshingGap));
        RequirePositive(InitialVelocityRangeFraction, nameof(InitialVelocityRangeFraction));
    }

    private static void RequirePositive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0.0)
            throw new ArgumentOutOfRangeException(name);
    }
}
