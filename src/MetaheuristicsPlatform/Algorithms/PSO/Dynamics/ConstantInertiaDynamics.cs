namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>
/// Constant inertia-weight dynamics.
/// </summary>
public sealed class ConstantInertiaDynamics :
    IPsoVelocityDynamics
{
    public ConstantInertiaDynamics(double inertiaWeight)
    {
        if (!double.IsFinite(inertiaWeight) ||
            inertiaWeight < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inertiaWeight));
        }

        InertiaWeight = inertiaWeight;
    }

    public double InertiaWeight { get; }

    public string Id => "constant-inertia";

    public PsoVelocityCoefficients GetCoefficients(
        long iteration) =>
        new(
            InertiaWeight,
            1.0);
}