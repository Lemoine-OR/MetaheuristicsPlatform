namespace MetaheuristicsPlatform.Algorithms.SalpSwarm;
public enum SalpSwarmAlgorithmPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }
public readonly record struct SalpSwarmAlgorithmState(int Iteration, SalpSwarmAlgorithmPhase Phase, int PopulationSize, double C1, double? FoodFitness);
