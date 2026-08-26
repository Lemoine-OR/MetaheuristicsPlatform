namespace MetaheuristicsPlatform.Algorithms.EquilibriumOptimizer;
public enum EquilibriumOptimizerPhase { Initialization=0, Search=1, CompletedIteration=2 }
public readonly record struct EquilibriumOptimizerState(int Iteration, EquilibriumOptimizerPhase Phase, double? BestFitness);
