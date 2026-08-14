namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Immutable common state exposed to stopping criteria and callbacks.
/// </summary>
/// <param name="Iteration">Number of completed iterations.</param>
/// <param name="Evaluations">Number of objective evaluations.</param>
/// <param name="Elapsed">Elapsed wall-clock time.</param>
/// <param name="HasBestSolution">Whether a best-so-far solution exists.</param>
/// <param name="BestFitness">Best-so-far fitness when available.</param>
/// <param name="LastImprovementIteration">Iteration index of the last improvement.</param>
/// <param name="LastImprovementEvaluation">Evaluation index of the last improvement.</param>
/// <param name="ImprovementCount">Number of strict best-so-far improvements.</param>
/// <param name="AlgorithmState">Optional algorithm-specific read-only state.</param>
/// <param name="LastImprovementElapsed">Elapsed time at the last strict improvement.</param>
public readonly record struct OptimizationState(
    long Iteration,
    long Evaluations,
    TimeSpan Elapsed,
    bool HasBestSolution,
    double BestFitness,
    long LastImprovementIteration,
    long LastImprovementEvaluation,
    long ImprovementCount,
    object? AlgorithmState = null,
    TimeSpan LastImprovementElapsed = default);