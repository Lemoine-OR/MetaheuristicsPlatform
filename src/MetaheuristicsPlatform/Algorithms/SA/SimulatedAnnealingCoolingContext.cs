namespace MetaheuristicsPlatform.Algorithms.SA;

public readonly record struct SimulatedAnnealingCoolingContext(
    long CompletedTemperatureLevels,
    long AttemptedTransitions,
    long AcceptedTransitions,
    double InitialTemperature,
    double CurrentTemperature)
{
    public double AcceptanceRatio =>
        AttemptedTransitions == 0
            ? 0.0
            : (double)AcceptedTransitions /
              AttemptedTransitions;
}