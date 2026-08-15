namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Statistical temperature decrement introduced by Huang, Romeo and
/// Sangiovanni-Vincentelli.
/// </summary>
/// <remarks>
/// T_(k+1) = T_k exp(-lambda T_k / sigma_k), 0 &lt; lambda &lt;= 1.
/// The original 1986 schedule also controls Markov-chain length and freezing;
/// this class implements the published temperature-decrement component.
/// </remarks>
public sealed class HuangStatisticalCoolingSchedule :
    ISimulatedAnnealingStatisticalCoolingSchedule
{
    public HuangStatisticalCoolingSchedule(
        double lambda = 0.6)
    {
        if (!double.IsFinite(lambda) ||
            lambda <= 0.0 ||
            lambda > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lambda));
        }

        Lambda = lambda;
    }

    public double Lambda { get; }

    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.HuangEtAl1986;

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

        return
            context.CurrentTemperature *
            Math.Exp(
                -Lambda *
                context.CurrentTemperature /
                sigma);
    }

    private static double RequireStandardDeviation(
        in SimulatedAnnealingCoolingContext context)
    {
        if (context.LevelObjectiveSamples < 2)
        {
            throw new InvalidOperationException(
                "Huang cooling requires at least two objective samples in the completed temperature level.");
        }

        double variance =
            context.LevelObjectiveVariance;

        if (!double.IsFinite(variance) ||
            variance < 0.0)
        {
            throw new InvalidOperationException(
                "Huang cooling requires a finite non-negative level objective variance.");
        }

        return
            Math.Sqrt(
                Math.Max(
                    0.0,
                    variance));
    }
}
