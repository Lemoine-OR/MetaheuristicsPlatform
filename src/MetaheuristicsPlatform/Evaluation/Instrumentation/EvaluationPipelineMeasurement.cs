namespace MetaheuristicsPlatform.Evaluation.Instrumentation;

/// <summary>
/// Raw Stopwatch-tick timings for one completed pipeline evaluation.
/// </summary>
public readonly record struct EvaluationPipelineMeasurement(
    long DecodeTicks,
    long RepairTicks,
    long ImproveTicks,
    long EvaluateTicks,
    long FeedbackTicks,
    long TotalTicks,
    bool WasRepaired,
    bool WasImproved,
    bool FeedbackApplied);