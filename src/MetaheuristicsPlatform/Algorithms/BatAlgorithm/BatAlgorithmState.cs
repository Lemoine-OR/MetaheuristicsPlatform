namespace MetaheuristicsPlatform.Algorithms.BatAlgorithm;

public enum BatAlgorithmPhase
{
    Initialization = 0,
    Search = 1,
    CompletedIteration = 2
}

public readonly record struct BatAlgorithmState(
    int Iteration, BatAlgorithmPhase Phase, int PopulationSize, int AcceptedMoves, double MeanLoudness, double? IterationBestFitness);
