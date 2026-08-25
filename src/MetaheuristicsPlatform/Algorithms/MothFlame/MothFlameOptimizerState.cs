namespace MetaheuristicsPlatform.Algorithms.MothFlame;
public enum MothFlameOptimizerPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }
public readonly record struct MothFlameOptimizerState(int Iteration, MothFlameOptimizerPhase Phase, int PopulationSize, int FlameCount, double A, double? BestFlameFitness);
