namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public readonly record struct LateAcceptanceState(
    double CurrentObjective,
    double BestObjective,
    double HistoryReference,
    int HistoryLength,
    int HistoryIndex,
    long AttemptedTransitions,
    long AcceptedTransitions,
    long ImprovingTransitions,
    long EqualTransitions,
    long WorseningTransitions,
    long DeltaEvaluations,
    long FullEvaluations,
    int ConsecutiveSamplingFailures);
