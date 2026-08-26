namespace MetaheuristicsPlatform.Algorithms.CrowSearch;

public enum CrowSearchPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }

public readonly record struct CrowSearchState(int Iteration, CrowSearchPhase Phase, int PopulationSize, int RandomRelocations, double? BestMemoryFitness);
