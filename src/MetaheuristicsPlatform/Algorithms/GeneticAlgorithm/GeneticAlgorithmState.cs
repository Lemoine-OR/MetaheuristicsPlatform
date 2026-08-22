namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Algorithm-specific state exposed through the common optimization lifecycle.
/// Counters are cumulative for the current run.
/// </summary>
public readonly record struct GeneticAlgorithmState(
    int Generation,
    int PopulationCount,
    long OffspringEvaluated,
    long ParentSelections,
    long CrossoverEvents,
    long MutationEvents,
    int EliteCount);
