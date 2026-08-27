using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.Speciation;

public sealed class SpeciesBasedPsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 50;
    public int MaximumIterations { get; init; } = 1000;
    public double SpeciesRadiusFraction { get; init; } = 0.1;
    public double InertiaWeight { get; init; } = 0.729844;
    public double CognitiveCoefficient { get; init; } = 1.49618;
    public double SocialCoefficient { get; init; } = 1.49618;
    public double InitialVelocityRangeFraction { get; init; } = 0.5;

    public void Validate()
    {
        if (SwarmSize <= 1) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(SpeciesRadiusFraction) ||
            SpeciesRadiusFraction <= 0.0 ||
            SpeciesRadiusFraction > 1.0)
            throw new ArgumentOutOfRangeException(nameof(SpeciesRadiusFraction));

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
