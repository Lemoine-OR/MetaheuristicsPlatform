namespace MetaheuristicsPlatform.Algorithms.Jaya;

public enum JayaPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }

public readonly record struct JayaState(int Iteration, JayaPhase Phase, int PopulationSize, double? BestFitness, double? WorstFitness);
