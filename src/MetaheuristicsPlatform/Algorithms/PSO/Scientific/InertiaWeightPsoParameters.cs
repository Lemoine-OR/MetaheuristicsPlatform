using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public sealed class InertiaWeightPsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 40;
    public int MaximumIterations { get; init; } = 1000;
    public double InertiaWeight { get; init; } = 1.0;
    public double CognitiveCoefficient { get; init; } = 2.0;
    public double SocialCoefficient { get; init; } = 2.0;
    public double InitialVelocityRangeFraction { get; init; } = 0.5;

    public void Validate()
    {
        if (SwarmSize <= 0) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        RequireFiniteNonNegative(InertiaWeight, nameof(InertiaWeight));
        RequireFiniteNonNegative(CognitiveCoefficient, nameof(CognitiveCoefficient));
        RequireFiniteNonNegative(SocialCoefficient, nameof(SocialCoefficient));
        RequireFiniteNonNegative(InitialVelocityRangeFraction, nameof(InitialVelocityRangeFraction));
    }

    private static void RequireFiniteNonNegative(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0.0)
            throw new ArgumentOutOfRangeException(name);
    }
}
