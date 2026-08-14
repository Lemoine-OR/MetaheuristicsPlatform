namespace MetaheuristicsPlatform.Algorithms.DE.State;

public readonly record struct DeIterationState(
    int PopulationSize,
    int Dimension,
    DeMutationStrategy MutationStrategy,
    DeCrossoverStrategy CrossoverStrategy,
    int AcceptedTrials)
{
    public double AcceptanceRatio =>
        PopulationSize == 0
            ? 0.0
            : (double)AcceptedTrials /
              PopulationSize;
}