namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>Observable runtime state for Threshold Accepting.</summary>
public readonly record struct ThresholdAcceptingState(
    double CurrentObjective,
    double BestObjective,
    double Threshold,
    long ThresholdLevel,
    int TransitionsInCurrentLevel,
    long AttemptedTransitions,
    long AcceptedTransitions,
    long ImprovingTransitions,
    long EqualTransitions,
    long WorseningTransitions,
    long DeltaEvaluations,
    long FullEvaluations,
    int ConsecutiveSamplingFailures);