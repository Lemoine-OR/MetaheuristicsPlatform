using System.Collections.ObjectModel;

namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Canonical runtime catalog of built-in SA cooling laws.
/// </summary>
public static class SimulatedAnnealingCoolingScheduleCatalog
{
    private static readonly ReadOnlyCollection<
        SimulatedAnnealingCoolingScheduleDescriptor> Entries =
        Array.AsReadOnly(
            new[]
            {
                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.Geometric,
                    "Geometric cooling",
                    SimulatedAnnealingCoolingScheduleKind.Geometric,
                    typeof(GeometricCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: false,
                    reference: SimulatedAnnealingReferences.KirkpatrickGelattVecchi1983,
                    scientificScope:
                        "Practical multiplicative baseline T_(k+1)=alpha T_k; no general Hajek-style convergence theorem is claimed for a fixed alpha."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.LundyMees1986,
                    "Lundy-Mees cooling",
                    SimulatedAnnealingCoolingScheduleKind.LundyMees,
                    typeof(LundyMeesCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: false,
                    reference: SimulatedAnnealingReferences.LundyMees1986,
                    scientificScope:
                        "Published rational recurrence T_(k+1)=T_k/(1+beta T_k)."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.Linear,
                    "Linear cooling",
                    SimulatedAnnealingCoolingScheduleKind.Linear,
                    typeof(LinearCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: false,
                    reference: SimulatedAnnealingReferences.StrenskiKirkpatrick1991,
                    scientificScope:
                        "Classical finite-length additive cooling T_(k+1)=max(0,T_k-beta); no logarithmic asymptotic guarantee is claimed."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.Hajek1988,
                    "Hajek logarithmic cooling",
                    SimulatedAnnealingCoolingScheduleKind.HajekLogarithmic,
                    typeof(HajekLogarithmicCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: false,
                    reference: SimulatedAnnealingReferences.Hajek1988,
                    scientificScope:
                        "Normalized member of the c/log(1+t) family used in Hajek's necessary-and-sufficient convergence analysis."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.SzuHartley1987,
                    "Szu-Hartley fast Cauchy cooling",
                    SimulatedAnnealingCoolingScheduleKind.SzuHartleyFastCauchy,
                    typeof(SzuHartleyFastCauchyCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: true,
                    reference: SimulatedAnnealingReferences.SzuHartley1987,
                    scientificScope:
                        "Implements the inverse-linear temperature law only; full FSA also requires the Cauchy visiting distribution."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.Ingber1989,
                    "Ingber very-fast cooling",
                    SimulatedAnnealingCoolingScheduleKind.IngberVeryFast,
                    typeof(IngberVeryFastCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: true,
                    reference: SimulatedAnnealingReferences.Ingber1989,
                    scientificScope:
                        "Implements T_k=T_0 exp(-c k^(1/D)); full VFSR/ASA includes generating-temperature adaptation and re-annealing."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.TsallisStariolo1996,
                    "Tsallis-Stariolo generalized cooling",
                    SimulatedAnnealingCoolingScheduleKind.TsallisStarioloGeneralized,
                    typeof(TsallisStarioloGeneralizedCoolingSchedule),
                    isAdaptive: false,
                    requiresLevelObjectiveStatistics: false,
                    isComponentOfBroaderAnnealingAlgorithm: true,
                    reference: SimulatedAnnealingReferences.TsallisStariolo1996,
                    scientificScope:
                        "Implements the GSA visiting-temperature law; full GSA also changes visiting and acceptance distributions."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.AartsVanLaarhoven1985,
                    "Aarts-van Laarhoven statistical cooling",
                    SimulatedAnnealingCoolingScheduleKind.AartsVanLaarhovenStatistical,
                    typeof(AartsVanLaarhovenStatisticalCoolingSchedule),
                    isAdaptive: true,
                    requiresLevelObjectiveStatistics: true,
                    isComponentOfBroaderAnnealingAlgorithm: false,
                    reference: SimulatedAnnealingReferences.AartsVanLaarhoven1985,
                    scientificScope:
                        "Temperature decrement adapts to the empirical objective standard deviation at the current level."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.HuangEtAl1986,
                    "Huang-Romeo-Sangiovanni statistical cooling",
                    SimulatedAnnealingCoolingScheduleKind.HuangStatistical,
                    typeof(HuangStatisticalCoolingSchedule),
                    isAdaptive: true,
                    requiresLevelObjectiveStatistics: true,
                    isComponentOfBroaderAnnealingAlgorithm: true,
                    reference: SimulatedAnnealingReferences.HuangRomeoSangiovanniVincentelli1986,
                    scientificScope:
                        "Implements the adaptive temperature decrement; the complete 1986 schedule also adapts Markov-chain length and detects freezing."),

                new SimulatedAnnealingCoolingScheduleDescriptor(
                    SimulatedAnnealingCoolingScheduleIds.TrikiEtAl2005,
                    "Triki-Collette-Siarry adaptive cooling",
                    SimulatedAnnealingCoolingScheduleKind.TrikiAdaptive,
                    typeof(TrikiAdaptiveCoolingSchedule),
                    isAdaptive: true,
                    requiresLevelObjectiveStatistics: true,
                    isComponentOfBroaderAnnealingAlgorithm: false,
                    reference: SimulatedAnnealingReferences.TrikiColletteSiarry2005,
                    scientificScope:
                        "Variance-driven decrement with explicit target mean-objective decrease Delta.")
            });

    public static IReadOnlyList<
        SimulatedAnnealingCoolingScheduleDescriptor> All =>
        Entries;

    public static SimulatedAnnealingCoolingScheduleDescriptor Get(
        SimulatedAnnealingCoolingScheduleKind kind)
    {
        foreach (SimulatedAnnealingCoolingScheduleDescriptor entry in
                 Entries)
        {
            if (entry.Kind == kind)
            {
                return entry;
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(kind));
    }

    public static SimulatedAnnealingCoolingScheduleDescriptor Get(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "A cooling-schedule ID is required.",
                nameof(id));
        }

        foreach (SimulatedAnnealingCoolingScheduleDescriptor entry in
                 Entries)
        {
            if (string.Equals(
                    entry.Id,
                    id,
                    StringComparison.Ordinal))
            {
                return entry;
            }
        }

        throw new KeyNotFoundException(
            $"Unknown simulated-annealing cooling-schedule ID '{id}'.");
    }
}
