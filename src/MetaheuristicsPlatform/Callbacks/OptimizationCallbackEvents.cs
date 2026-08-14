namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Selects callback events to emit. Evaluation events are disabled by default because they may be very frequent.
/// </summary>
[Flags]
public enum OptimizationCallbackEvents
{
    None = 0,
    Started = 1 << 0,
    EvaluationCompleted = 1 << 1,
    BestImproved = 1 << 2,
    IterationCompleted = 1 << 3,
    Completed = 1 << 4,
    All = Started | EvaluationCompleted | BestImproved | IterationCompleted | Completed
}