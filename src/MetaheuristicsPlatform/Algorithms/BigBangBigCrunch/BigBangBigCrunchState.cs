namespace MetaheuristicsPlatform.Algorithms.BigBangBigCrunch;

public enum BigBangBigCrunchPhase { Initialization = 0, BigBang = 1, CompletedIteration = 2 }

public readonly record struct BigBangBigCrunchState(int Iteration, BigBangBigCrunchPhase Phase, int PopulationSize, double RadiusFactor, double? RepresentativeFitness);
