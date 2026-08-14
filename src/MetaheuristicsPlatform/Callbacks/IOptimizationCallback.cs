namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Receives standardized optimization events.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public interface IOptimizationCallback<TSolution>
{
    /// <summary>
    /// Gets the event types consumed by this callback.
    /// The common runtime avoids invoking the callback for other event types.
    /// </summary>
    OptimizationCallbackEvents Events => OptimizationCallbackEvents.All;

    /// <summary>Processes one optimization lifecycle event.</summary>
    void OnEvent(in OptimizationEvent<TSolution> optimizationEvent);
}