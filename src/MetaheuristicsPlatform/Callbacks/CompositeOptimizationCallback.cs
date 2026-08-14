namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Fans one standardized event out to several callbacks.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public sealed class CompositeOptimizationCallback<TSolution> : IOptimizationCallback<TSolution>
{
    private readonly IReadOnlyList<IOptimizationCallback<TSolution>> _callbacks;

    /// <summary>Initializes the composite callback.</summary>
    public CompositeOptimizationCallback(params IOptimizationCallback<TSolution>[] callbacks)
    {
        ArgumentNullException.ThrowIfNull(callbacks);
        if (callbacks.Any(static callback => callback is null))
        {
            throw new ArgumentException("Callback collection cannot contain null elements.", nameof(callbacks));
        }

        _callbacks = callbacks;

        OptimizationCallbackEvents events = OptimizationCallbackEvents.None;
        foreach (IOptimizationCallback<TSolution> callback in callbacks)
        {
            events |= callback.Events;
        }

        Events = events;
    }

    /// <inheritdoc />
    public OptimizationCallbackEvents Events { get; }

    /// <inheritdoc />
    public void OnEvent(in OptimizationEvent<TSolution> optimizationEvent)
    {
        OptimizationCallbackEvents eventFlag = optimizationEvent.Kind.ToCallbackFlag();

        foreach (IOptimizationCallback<TSolution> callback in _callbacks)
        {
            if ((callback.Events & eventFlag) != 0)
            {
                callback.OnEvent(in optimizationEvent);
            }
        }
    }
}