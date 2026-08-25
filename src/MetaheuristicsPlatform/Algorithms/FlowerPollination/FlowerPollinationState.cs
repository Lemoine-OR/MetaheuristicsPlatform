namespace MetaheuristicsPlatform.Algorithms.FlowerPollination;

public enum FlowerPollinationPhase
{
    Initialization = 0,
    Search = 1,
    CompletedIteration = 2
}

public readonly record struct FlowerPollinationState(
    int Iteration, FlowerPollinationPhase Phase, int PopulationSize, int GlobalPollinations, int LocalPollinations, double? IterationBestFitness);
