namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Marker for cooling schedules that require objective statistics measured
/// over the just-completed fixed-temperature level.
/// </summary>
public interface ISimulatedAnnealingStatisticalCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
}
