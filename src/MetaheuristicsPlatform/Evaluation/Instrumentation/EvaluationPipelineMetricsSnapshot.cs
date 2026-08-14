namespace MetaheuristicsPlatform.Evaluation.Instrumentation;

/// <summary>
/// Atomic point-in-time snapshot of aggregate pipeline metrics.
/// Tick values use <see cref="System.Diagnostics.Stopwatch.Frequency"/>.
/// </summary>
public readonly record struct EvaluationPipelineMetricsSnapshot(
    long EvaluationCount,
    long RepairCount,
    long ImprovementCount,
    long FeedbackCount,
    long CacheHitCount,
    long CacheMissCount,
    long DecodeTicks,
    long RepairTicks,
    long ImproveTicks,
    long EvaluateTicks,
    long FeedbackTicks,
    long TotalTicks)
{
    public double CacheHitRatio =>
        CacheHitCount + CacheMissCount == 0
            ? 0.0
            : (double)CacheHitCount /
              (CacheHitCount + CacheMissCount);

    public TimeSpan TotalTime =>
        FromStopwatchTicks(TotalTicks);

    public TimeSpan DecodeTime =>
        FromStopwatchTicks(DecodeTicks);

    public TimeSpan RepairTime =>
        FromStopwatchTicks(RepairTicks);

    public TimeSpan ImproveTime =>
        FromStopwatchTicks(ImproveTicks);

    public TimeSpan EvaluateTime =>
        FromStopwatchTicks(EvaluateTicks);

    public TimeSpan FeedbackTime =>
        FromStopwatchTicks(FeedbackTicks);

    private static TimeSpan FromStopwatchTicks(long ticks) =>
        TimeSpan.FromSeconds(
            (double)ticks /
            System.Diagnostics.Stopwatch.Frequency);
}