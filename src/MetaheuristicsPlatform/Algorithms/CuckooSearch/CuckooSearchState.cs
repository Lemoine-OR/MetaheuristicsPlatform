namespace MetaheuristicsPlatform.Algorithms.CuckooSearch;

public enum CuckooSearchPhase
{
    Initialization = 0,
    Search = 1,
    CompletedIteration = 2
}

public readonly record struct CuckooSearchState(
    int Iteration, CuckooSearchPhase Phase, int NestCount, int LevyFlights, int AbandonedNests, double? IterationBestFitness);
