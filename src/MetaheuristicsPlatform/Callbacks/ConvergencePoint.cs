namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// One lightweight point in an optional convergence trace.
/// </summary>
public readonly record struct ConvergencePoint(
    OptimizationEventKind EventKind,
    long Iteration,
    long Evaluations,
    TimeSpan Elapsed,
    double BestFitness,
    double? CurrentFitness);