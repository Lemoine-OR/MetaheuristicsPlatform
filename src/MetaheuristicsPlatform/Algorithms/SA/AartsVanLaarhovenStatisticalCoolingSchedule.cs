namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Statistical cooling decrement associated with Aarts and van Laarhoven.
/// </summary>
/// <remarks>
/// T_(k+1) = T_k / (1 + T_k ln(1+delta)/(3 sigma_k)),
/// where sigma_k is the empirical standard deviation of objective values at
/// the current temperature level.
/// </remarks>
public sealed class AartsVanLaarhovenStatisticalCoolingSchedule :
    ISimulatedAnnealingStatisticalCoolingSchedule
{
    public AartsVanLaarhovenStatisticalCoolingSchedule(
        double delta = 0.1)
    {
        if (!double.IsFinite(delta) ||
            delta <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delta));
        }

        Delta = delta;
    }

    public double Delta { get; }

    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.AartsVanLaarhoven1985;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        double sigma =
            RequireStandardDeviation(
                in context);

        if (sigma == 0.0)
        {
            return 0.0;
        }

        double denominator =
            1.0 +
            context.CurrentTemperature *
            Math.Log(
                1.0 +
                Delta) /
            (3.0 *
             sigma);

        return
            context.CurrentTemperature /
            denominator;
    }

    private static double RequireStandardDeviation(
        in SimulatedAnnealingCoolingContext context)
    {
        if (context.LevelObjectiveSamples < 2)
        {
            throw new InvalidOperationException(
                "Aarts-van Laarhoven cooling requires at least two objective samples in the completed temperature level.");
        }

        double variance =
            context.LevelObjectiveVariance;

        if (!double.IsFinite(variance) ||
            variance < 0.0)
        {
            throw new InvalidOperationException(
                "Aarts-van Laarhoven cooling requires a finite non-negative level objective variance.");
        }

        return
            Math.Sqrt(
                Math.Max(
                    0.0,
                    variance));
    }
}
