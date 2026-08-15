namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Stable public identifiers for Tabu Search memory and control components.
/// </summary>
public static class TabuSearchComponentIds
{
    public const string ShortTermExpirationMemory =
        "ts.memory.short-term.expiration";
    public const string AttributeFrequencyMemory =
        "ts.memory.frequency.attribute";
    public const string ConfigurationRepetitionHashMemory =
        "ts.memory.repetition.hash";
    public const string FixedTenure =
        "ts.tenure.fixed";
    public const string UniformRandomTenure =
        "ts.tenure.uniform-random";
    public const string ReactiveTenure =
        "ts.tenure.reactive-battiti-tecchiolli-1994";
    public const string BestSoFarAspiration =
        "ts.aspiration.best-so-far";
    public const string EliteRestartIntensification =
        "ts.control.intensification.elite-restart";
    public const string FrequencyPenaltyDiversification =
        "ts.control.diversification.frequency-penalty";
    public const string ReactiveRandomWalkDiversification =
        "ts.control.diversification.reactive-random-walk";
}
