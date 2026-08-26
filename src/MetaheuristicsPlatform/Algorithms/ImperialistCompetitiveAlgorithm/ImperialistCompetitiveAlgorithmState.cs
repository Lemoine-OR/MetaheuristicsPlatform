namespace MetaheuristicsPlatform.Algorithms.ImperialistCompetitiveAlgorithm;
public enum ImperialistCompetitiveAlgorithmPhase { Initialization=0, Assimilation=1, Revolution=2, Competition=3, CompletedIteration=4 }
public readonly record struct ImperialistCompetitiveAlgorithmState(int Iteration, ImperialistCompetitiveAlgorithmPhase Phase, int EmpireCount, double? BestFitness);
