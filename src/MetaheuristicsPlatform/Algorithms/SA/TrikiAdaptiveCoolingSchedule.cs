namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Adaptive decrement proposed by Triki, Collette and Siarry.
/// </summary>
/// <remarks>
/// T_(k+1) = T_k (1 - T_k Delta / Var_k).
/// Delta is an explicit target decrement scale and Var_k is the empirical
/// objective variance at the current temperature. If the finite-sample
/// estimate would make the physical temperature non-positive, the law is
/// saturated at zero and the optimizer subsequently applies its configured
/// minimum-temperature floor.
/// DOI: 10.1016/j.ejor.2004.03.035.
/// </remarks>
public sealed class TrikiAdaptiveCoolingSchedule :
    ISimulatedAnnealingStatisticalCoolingSchedule
{
    public TrikiAdaptiveCoolingSchedule(
        double targetMeanObjectiveDecrease)
    {
        if (!double.IsFinite(
                targetMeanObjectiveDecrease) ||
            targetMeanObjectiveDecrease <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetMeanObjectiveDecrease));
        }

        TargetMeanObjectiveDecrease =
            targetMeanObjectiveDecrease;
    }

    public double TargetMeanObjectiveDecrease { get; }

    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.TrikiEtAl2005;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        if (context.LevelObjectiveSamples < 2)
        {
            throw new InvalidOperationException(
                "Triki cooling requires at least two objective samples in the completed temperature level.");
        }

        double variance =
            context.LevelObjectiveVariance;

        if (!double.IsFinite(variance) ||
            variance < 0.0)
        {
            throw new InvalidOperationException(
                "Triki cooling requires a finite non-negative level objective variance.");
        }

        if (variance == 0.0)
        {
            return 0.0;
        }

        double factor =
            1.0 -
            context.CurrentTemperature *
            TargetMeanObjectiveDecrease /
            variance;

        if (factor <= 0.0)
        {
            return 0.0;
        }

        return
            context.CurrentTemperature *
            factor;
    }
}
