namespace MetaheuristicsPlatform.Algorithms.WhaleOptimization;
public enum WhaleOptimizationAlgorithmPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }
public readonly record struct WhaleOptimizationAlgorithmState(int Iteration, WhaleOptimizationAlgorithmPhase Phase, int PopulationSize, double A, double? BestFitness);
