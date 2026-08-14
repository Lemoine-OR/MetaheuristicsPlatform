namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>
/// Multipliers applied to previous velocity and stochastic attraction.
/// </summary>
public readonly record struct PsoVelocityCoefficients(
    double PreviousVelocityMultiplier,
    double AttractionMultiplier);