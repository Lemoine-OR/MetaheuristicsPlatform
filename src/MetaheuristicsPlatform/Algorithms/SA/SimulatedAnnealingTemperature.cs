namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Exact temperature conversions derived from the Metropolis acceptance equation.
/// </summary>
public static class SimulatedAnnealingTemperature
{
    /// <summary>
    /// Returns T such that a worsening transition of the supplied degradation is
    /// accepted with the requested probability under exp(-delta/T).
    /// </summary>
    public static double FromWorseningAcceptanceProbability(
        double degradation,
        double targetAcceptanceProbability)
    {
        if (!double.IsFinite(degradation) ||
            degradation <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(degradation));
        }

        if (!double.IsFinite(
                targetAcceptanceProbability) ||
            targetAcceptanceProbability <= 0.0 ||
            targetAcceptanceProbability >= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetAcceptanceProbability));
        }

        return
            -degradation /
            Math.Log(
                targetAcceptanceProbability);
    }
}