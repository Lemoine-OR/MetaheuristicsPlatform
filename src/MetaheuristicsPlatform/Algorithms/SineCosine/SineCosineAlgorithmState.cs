namespace MetaheuristicsPlatform.Algorithms.SineCosine;
public enum SineCosineAlgorithmPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }
public readonly record struct SineCosineAlgorithmState(int Iteration, SineCosineAlgorithmPhase Phase, int PopulationSize, double R1, double? BestFitness);
