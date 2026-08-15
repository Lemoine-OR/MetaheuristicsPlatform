namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Immutable information supplied to a simulated-annealing cooling schedule.
/// </summary>
/// <remarks>
/// The original five positional members are intentionally preserved. v0.20
/// adds per-level statistics as init-only members so existing callers remain
/// source-compatible.
/// </remarks>
public readonly record struct SimulatedAnnealingCoolingContext(
    long CompletedTemperatureLevels,
    long AttemptedTransitions,
    long AcceptedTransitions,
    double InitialTemperature,
    double CurrentTemperature)
{
    public double AcceptanceRatio =>
        AttemptedTransitions == 0
            ? 0.0
            : (double)AcceptedTransitions /
              AttemptedTransitions;

    /// <summary>
    /// Attempted transitions during the just-completed temperature level.
    /// </summary>
    public long LevelAttemptedTransitions { get; init; }

    /// <summary>
    /// Accepted transitions during the just-completed temperature level.
    /// </summary>
    public long LevelAcceptedTransitions { get; init; }

    /// <summary>
    /// Acceptance ratio for the just-completed temperature level.
    /// </summary>
    public double LevelAcceptanceRatio =>
        LevelAttemptedTransitions == 0
            ? 0.0
            : (double)LevelAcceptedTransitions /
              LevelAttemptedTransitions;

    /// <summary>
    /// Number of objective-state samples used to estimate level statistics.
    /// Zero means that statistics were not requested by the schedule.
    /// </summary>
    public long LevelObjectiveSamples { get; init; }

    /// <summary>
    /// Empirical mean objective over the just-completed level.
    /// </summary>
    public double LevelObjectiveMean { get; init; }

    /// <summary>
    /// Empirical population variance of objective values over the
    /// just-completed level.
    /// </summary>
    public double LevelObjectiveVariance { get; init; }

    /// <summary>
    /// Empirical population standard deviation of objective values over the
    /// just-completed level.
    /// </summary>
    public double LevelObjectiveStandardDeviation =>
        LevelObjectiveSamples < 2
            ? double.NaN
            : Math.Sqrt(
                Math.Max(
                    0.0,
                    LevelObjectiveVariance));
}
