namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Runtime state shared by the advanced ACO algorithms.</summary>
public readonly record struct AdvancedAntColonyState(
    int IterationsCompleted,
    long AntsConstructed,
    long ConstructionSteps,
    long TransitionEvaluations,
    int PheromoneEntries,
    long GlobalPheromoneUpdates,
    long LocalPheromoneUpdates,
    int Restarts,
    int ConsecutiveNonImprovingIterations,
    double? LastIterationBestObjective);
