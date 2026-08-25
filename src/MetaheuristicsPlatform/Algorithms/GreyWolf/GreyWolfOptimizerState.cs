namespace MetaheuristicsPlatform.Algorithms.GreyWolf;

public enum GreyWolfOptimizerPhase
{
    Initialization = 0,
    Search = 1,
    CompletedIteration = 2
}

public readonly record struct GreyWolfOptimizerState(
    int Iteration, GreyWolfOptimizerPhase Phase, int PopulationSize, double A, double? AlphaFitness, double? BetaFitness, double? DeltaFitness);
