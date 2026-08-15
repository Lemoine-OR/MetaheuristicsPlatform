namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Built-in simulated-annealing cooling laws.
/// Existing numeric values are preserved for backward compatibility.
/// </summary>
public enum SimulatedAnnealingCoolingScheduleKind
{
    Geometric = 0,
    LundyMees = 1,
    Linear = 2,
    HajekLogarithmic = 3,
    SzuHartleyFastCauchy = 4,
    IngberVeryFast = 5,
    TsallisStarioloGeneralized = 6,
    AartsVanLaarhovenStatistical = 7,
    HuangStatistical = 8,
    TrikiAdaptive = 9
}
