namespace MetaheuristicsPlatform.Algorithms.HarrisHawks;
public enum HarrisHawksOptimizerPhase { Initialization = 0, Search = 1, CompletedIteration = 2 }
public readonly record struct HarrisHawksOptimizerState(int Iteration, HarrisHawksOptimizerPhase Phase, int PopulationSize, double E1, int RapidDiveEvaluations, double? RabbitFitness);
