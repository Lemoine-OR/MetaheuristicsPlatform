namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Adapts a delegate to the standard optimization callback contract.
/// </summary>
public sealed class DelegateOptimizationCallback<TSolution> : IOptimizationCallback<TSolution>
{
    private readonly OptimizationEventHandler<TSolution> _handler;

    /// <summary>Initializes a delegate callback.</summary>
    public DelegateOptimizationCallback(
        OptimizationEventHandler<TSolution> handler,
        OptimizationCallbackEvents events = OptimizationCallbackEvents.All)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        Events = events;
    }

    /// <inheritdoc />
    public OptimizationCallbackEvents Events { get; }

    /// <inheritdoc />
    public void OnEvent(in OptimizationEvent<TSolution> optimizationEvent) =>
        _handler(in optimizationEvent);
}