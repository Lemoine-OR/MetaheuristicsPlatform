namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Inverse-linear cooling law used by Szu-Hartley fast simulated annealing.
/// </summary>
/// <remarks>
/// T_k = T_0 / (k + 1), where k is the number of completed temperature levels.
/// In the original Fast Simulated Annealing algorithm this law is coupled to a
/// Cauchy visiting distribution. This class implements only the temperature
/// law and does not claim to reproduce the full FSA algorithm.
/// DOI: 10.1016/0375-9601(87)90796-1.
/// </remarks>
public sealed class SzuHartleyFastCauchyCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.SzuHartley1987;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context) =>
        context.InitialTemperature /
        (context.CompletedTemperatureLevels +
         1.0);
}
