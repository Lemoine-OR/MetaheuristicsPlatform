namespace MetaheuristicsPlatform.Callbacks;

/// <summary>Maps lifecycle event kinds to callback-selection flags.</summary>
public static class OptimizationEventKindExtensions
{
    /// <summary>Gets the corresponding callback flag.</summary>
    public static OptimizationCallbackEvents ToCallbackFlag(this OptimizationEventKind kind) =>
        kind switch
        {
            OptimizationEventKind.Started => OptimizationCallbackEvents.Started,
            OptimizationEventKind.EvaluationCompleted => OptimizationCallbackEvents.EvaluationCompleted,
            OptimizationEventKind.BestImproved => OptimizationCallbackEvents.BestImproved,
            OptimizationEventKind.IterationCompleted => OptimizationCallbackEvents.IterationCompleted,
            OptimizationEventKind.Completed => OptimizationCallbackEvents.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported optimization event kind.")
        };
}