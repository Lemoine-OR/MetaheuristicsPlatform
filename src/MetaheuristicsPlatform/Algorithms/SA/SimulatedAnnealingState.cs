namespace MetaheuristicsPlatform.Algorithms.SA;

public readonly record struct SimulatedAnnealingState(
    double CurrentObjective,
    double BestObjective,
    double Temperature,
    long TemperatureLevel,
    int TransitionsInCurrentLevel,
    long AttemptedTransitions,
    long AcceptedTransitions,
    long ImprovingTransitions,
    long EqualTransitions,
    long WorseningTransitions,
    long DeltaEvaluations,
    long FullEvaluations,
    int ConsecutiveSamplingFailures)
{
    public double AcceptanceRatio =>
        AttemptedTransitions == 0
            ? 0.0
            : (double)AcceptedTransitions /
              AttemptedTransitions;
}