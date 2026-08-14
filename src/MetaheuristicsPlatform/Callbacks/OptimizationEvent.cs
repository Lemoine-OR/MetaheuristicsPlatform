using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Callbacks;

/// <summary>
/// Immutable standardized callback payload shared by every metaheuristic.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public readonly record struct OptimizationEvent<TSolution>(
    OptimizationEventKind Kind,
    OptimizationState State,
    TSolution? BestSolution,
    double? CurrentFitness = null,
    object? AlgorithmData = null);