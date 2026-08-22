namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Observable Ant System state for callbacks and custom stopping criteria.</summary>
public readonly record struct AntSystemState(
    int IterationsCompleted,
    long AntsConstructed,
    long ConstructionSteps,
    long TransitionEvaluations,
    int PheromoneEntries,
    int EvaporationRounds,
    long PheromoneDepositApplications,
    double? LastIterationBestObjective);
