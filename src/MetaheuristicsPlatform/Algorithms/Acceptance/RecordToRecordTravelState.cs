namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public readonly record struct RecordToRecordTravelState(
    double CurrentObjective,
    double RecordObjective,
    double Deviation,
    long AttemptedTransitions,
    long AcceptedTransitions,
    long ImprovingTransitions,
    long EqualTransitions,
    long WorseningTransitions,
    long DeltaEvaluations,
    long FullEvaluations,
    int ConsecutiveSamplingFailures);