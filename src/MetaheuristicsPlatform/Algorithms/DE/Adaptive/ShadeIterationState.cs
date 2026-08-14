namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public readonly record struct ShadeIterationState(
    int PopulationSize,
    int Dimension,
    int SuccessfulTrials,
    int ArchiveCount,
    int MemoryPosition,
    double MeanMemoryDifferentialWeight,
    double MeanMemoryCrossoverProbability)
{
    public double SuccessRatio =>
        PopulationSize == 0
            ? 0.0
            : (double)SuccessfulTrials /
              PopulationSize;
}