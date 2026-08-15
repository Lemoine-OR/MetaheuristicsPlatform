namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Normalized logarithmic cooling law in the Hajek/Geman-Geman family.
/// </summary>
/// <remarks>
/// With k equal to the number of completed temperature levels,
/// T_k = T_0 ln(2) / ln(k + 2).
/// This is the shifted normalization of c / ln(1+t) that preserves T_0 at
/// artificial time t=1. Hajek's convergence result requires the constant c
/// to be at least the critical depth of the deepest non-global local minimum
/// under the theorem's communication assumptions.
/// DOI: 10.1287/moor.13.2.311.
/// </remarks>
public sealed class HajekLogarithmicCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.Hajek1988;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        double denominator =
            Math.Log(
                context.CompletedTemperatureLevels +
                2.0);

        return
            context.InitialTemperature *
            Math.Log(2.0) /
            denominator;
    }
}
