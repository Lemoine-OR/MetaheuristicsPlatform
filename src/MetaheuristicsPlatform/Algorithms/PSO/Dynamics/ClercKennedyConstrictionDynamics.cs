namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>
/// Clerc-Kennedy constriction dynamics.
/// </summary>
public sealed class ClercKennedyConstrictionDynamics :
    IPsoVelocityDynamics
{
    public ClercKennedyConstrictionDynamics(
        double phi = 4.10,
        double kappa = 1.0)
    {
        Phi = phi;
        Kappa = kappa;
        Chi =
            PsoConstrictionFactor.Compute(
                phi,
                kappa);
    }

    public double Phi { get; }
    public double Kappa { get; }
    public double Chi { get; }

    public string Id => "clerc-kennedy-constriction";

    public PsoVelocityCoefficients GetCoefficients(
        long iteration) =>
        new(
            Chi,
            Chi);
}