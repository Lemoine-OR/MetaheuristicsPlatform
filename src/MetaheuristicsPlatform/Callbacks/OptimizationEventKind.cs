namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Identifies a standardized optimization lifecycle event.
/// </summary>
public enum OptimizationEventKind
{
    Started = 0,
    EvaluationCompleted = 1,
    BestImproved = 2,
    IterationCompleted = 3,
    Completed = 4
}