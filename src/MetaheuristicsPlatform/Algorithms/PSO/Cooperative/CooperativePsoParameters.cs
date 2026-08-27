using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.Cooperative;

public sealed class CooperativePsoParameters : IMetaheuristicParameters
{
    public int SubswarmCount { get; init; } = 6;
    public int SubswarmSize { get; init; } = 20;
    public int MaximumIterations { get; init; } = 1000;
    public double InertiaWeight { get; init; } = 0.729844;
    public double CognitiveCoefficient { get; init; } = 1.49618;
    public double SocialCoefficient { get; init; } = 1.49618;
    public double InitialVelocityRangeFraction { get; init; } = 0.5;

    public void Validate()
    {
        if (SubswarmCount <= 0) throw new ArgumentOutOfRangeException(nameof(SubswarmCount));
        if (SubswarmSize <= 1) throw new ArgumentOutOfRangeException(nameof(SubswarmSize));
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
