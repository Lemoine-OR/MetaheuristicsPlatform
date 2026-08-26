namespace MetaheuristicsPlatform.Algorithms.MultiVerseOptimizer;
public enum MultiVerseOptimizerPhase { Initialization=0, WhiteHoleAndWormhole=1, CompletedIteration=2 }
public readonly record struct MultiVerseOptimizerState(int Iteration, MultiVerseOptimizerPhase Phase, double WormholeExistenceProbability, double TravellingDistanceRate, double? BestInflationRate);
