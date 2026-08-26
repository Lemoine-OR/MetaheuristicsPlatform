namespace MetaheuristicsPlatform.Algorithms.GravitationalSearch;

public enum GravitationalSearchPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }

public readonly record struct GravitationalSearchState(int Iteration, GravitationalSearchPhase Phase, int PopulationSize, int KBest, double Gravity, double? BestFitness);
