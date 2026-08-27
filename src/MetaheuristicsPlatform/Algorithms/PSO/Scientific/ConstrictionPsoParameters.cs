using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public sealed class ConstrictionPsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 40;
    public int MaximumIterations { get; init; } = 1000;
    public double CognitiveCoefficient { get; init; } = 2.05;
    public double SocialCoefficient { get; init; } = 2.05;
    public double Kappa { get; init; } = 1.0;
    public double InitialVelocityRangeFraction { get; init; } = 0.5;
    public double Phi => CognitiveCoefficient + SocialCoefficient;

    public void Validate()
    {
        if (SwarmSize <= 0) throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        RequirePositive(CognitiveCoefficient, nameof(CognitiveCoefficient));
        RequirePositive(SocialCoefficient, nameof(SocialCoefficient));
        RequirePositive(Kappa, nameof(Kappa));
        if (!(Phi > 4.0)) throw new ArgumentOutOfRangeException(nameof(Phi));
        if (!double.IsFinite(InitialVelocityRangeFraction) || InitialVelocityRangeFraction < 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialVelocityRangeFraction));
    }

    private static void RequirePositive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0.0)
            throw new ArgumentOutOfRangeException(name);
    }
}
