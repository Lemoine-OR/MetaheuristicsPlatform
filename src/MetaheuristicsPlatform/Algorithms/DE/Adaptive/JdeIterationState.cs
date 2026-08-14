namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public readonly record struct JdeIterationState(
    int PopulationSize,
    int Dimension,
    int AcceptedTrials,
    double MeanDifferentialWeight,
    double MeanCrossoverProbability)
{
    public double AcceptanceRatio =>
        PopulationSize == 0
            ? 0.0
            : (double)AcceptedTrials /
              PopulationSize;
}