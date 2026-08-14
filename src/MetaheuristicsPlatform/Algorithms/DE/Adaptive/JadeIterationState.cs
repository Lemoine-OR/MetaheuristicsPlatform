namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public readonly record struct JadeIterationState(
    int PopulationSize,
    int Dimension,
    int SuccessfulTrials,
    int ArchiveCount,
    double MeanDifferentialWeight,
    double MeanCrossoverProbability)
{
    public double SuccessRatio =>
        PopulationSize == 0
            ? 0.0
            : (double)SuccessfulTrials /
              PopulationSize;
}