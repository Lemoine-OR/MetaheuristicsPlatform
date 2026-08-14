namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Common statistics produced by every optimization run.
/// </summary>
public readonly record struct OptimizationRunStatistics(
    long Iterations,
    long Evaluations,
    long Improvements,
    TimeSpan Elapsed,
    long LastImprovementIteration,
    long LastImprovementEvaluation,
    TimeSpan LastImprovementElapsed = default)
{
    /// <summary>Gets objective evaluations per elapsed second.</summary>
    public double EvaluationsPerSecond =>
        Elapsed.TotalSeconds > 0.0 ? Evaluations / Elapsed.TotalSeconds : 0.0;

    /// <summary>Gets completed iterations per elapsed second.</summary>
    public double IterationsPerSecond =>
        Elapsed.TotalSeconds > 0.0 ? Iterations / Elapsed.TotalSeconds : 0.0;
}