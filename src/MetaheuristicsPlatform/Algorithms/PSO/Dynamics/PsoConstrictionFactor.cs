namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>
/// Clerc-Kennedy constriction factor.
/// </summary>
public static class PsoConstrictionFactor
{
    /// <summary>
    /// Computes chi = 2*kappa / |2 - phi - sqrt(phi^2 - 4*phi)|.
    /// The convergence analysis requires phi &gt; 4 and 0 &lt; kappa &lt;= 1.
    /// </summary>
    public static double Compute(
        double phi,
        double kappa = 1.0)
    {
        if (!double.IsFinite(phi) ||
            phi <= 4.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(phi),
                "phi must be finite and strictly greater than 4.");
        }

        if (!double.IsFinite(kappa) ||
            kappa <= 0.0 ||
            kappa > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kappa));
        }

        double discriminant =
            Math.Sqrt(
                (phi * phi) -
                (4.0 * phi));

        return
            (2.0 * kappa) /
            Math.Abs(
                2.0 -
                phi -
                discriminant);
    }
}