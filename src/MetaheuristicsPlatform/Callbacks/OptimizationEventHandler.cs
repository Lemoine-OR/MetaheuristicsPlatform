namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Strongly typed zero-boxing callback delegate for optimization events.
/// </summary>
public delegate void OptimizationEventHandler<TSolution>(
    in OptimizationEvent<TSolution> optimizationEvent);