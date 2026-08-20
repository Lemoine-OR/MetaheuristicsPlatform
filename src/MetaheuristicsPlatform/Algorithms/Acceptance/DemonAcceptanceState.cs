namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public readonly record struct DemonAcceptanceState(
    double CurrentObjective,
    double BestObjective,
    double Credit,
    double InitialCredit,
    long AttemptedTransitions,
    long AcceptedTransitions,
    long ImprovingTransitions,
    long EqualTransitions,
    long WorseningTransitions,
    long DeltaEvaluations,
    long FullEvaluations,
    int ConsecutiveSamplingFailures);
