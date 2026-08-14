namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Optional reusable convergence recorder.
/// No convergence history is allocated by the Core unless this callback is attached.
/// </summary>
public sealed class ConvergenceTraceCallback<TSolution> : IOptimizationCallback<TSolution>
{
    private readonly List<ConvergencePoint> _points = [];
    private readonly object _gate = new();
    private readonly int? _maxPoints;

    /// <summary>
    /// Initializes a recorder.
    /// </summary>
    /// <param name="events">Events to record.</param>
    /// <param name="maxPoints">Optional hard cap on stored points.</param>
    public ConvergenceTraceCallback(
        OptimizationCallbackEvents events =
            OptimizationCallbackEvents.BestImproved |
            OptimizationCallbackEvents.IterationCompleted |
            OptimizationCallbackEvents.Completed,
        int? maxPoints = null)
    {
        if (maxPoints.HasValue && maxPoints.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPoints));
        }

        Events = events;
        _maxPoints = maxPoints;
    }

    /// <inheritdoc />
    public OptimizationCallbackEvents Events { get; }

    /// <summary>Gets a snapshot of recorded convergence points.</summary>
    public IReadOnlyList<ConvergencePoint> GetSnapshot()
    {
        lock (_gate)
        {
            return _points.ToArray();
        }
    }

    /// <inheritdoc />
    public void OnEvent(in OptimizationEvent<TSolution> optimizationEvent)
    {
        if (!optimizationEvent.State.HasBestSolution)
        {
            return;
        }

        lock (_gate)
        {
            if (_maxPoints.HasValue && _points.Count >= _maxPoints.Value)
            {
                return;
            }

            _points.Add(new ConvergencePoint(
                optimizationEvent.Kind,
                optimizationEvent.State.Iteration,
                optimizationEvent.State.Evaluations,
                optimizationEvent.State.Elapsed,
                optimizationEvent.State.BestFitness,
                optimizationEvent.CurrentFitness));
        }
    }
}