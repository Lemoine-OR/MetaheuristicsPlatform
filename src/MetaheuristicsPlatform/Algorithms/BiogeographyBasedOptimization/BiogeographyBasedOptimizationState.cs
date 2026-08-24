namespace MetaheuristicsPlatform.Algorithms.BiogeographyBasedOptimization;

public enum BiogeographyBasedOptimizationPhase
{
    Initialization = 0,
    Search = 1,
    CompletedIteration = 2
}

public readonly record struct BiogeographyBasedOptimizationState(
    int Iteration, BiogeographyBasedOptimizationPhase Phase, int PopulationSize, int MigrationEvents, int MutationEvents, double? IterationBestFitness);
