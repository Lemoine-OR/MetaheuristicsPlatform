namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public readonly record struct GreatDelugeState(
    double CurrentObjective,
    double BestObjective,
    double WaterLevel,
    double RainSpeed,
    long AttemptedTransitions,
    long AcceptedTransitions,
    long ImprovingTransitions,
    long EqualTransitions,
    long WorseningTransitions,
    long DeltaEvaluations,
    long FullEvaluations,
    int ConsecutiveSamplingFailures);