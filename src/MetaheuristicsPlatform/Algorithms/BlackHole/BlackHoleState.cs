namespace MetaheuristicsPlatform.Algorithms.BlackHole;
public enum BlackHolePhase { Initialization=0, Attraction=1, EventHorizon=2, CompletedIteration=3 }
public readonly record struct BlackHoleState(int Iteration, BlackHolePhase Phase, int PopulationSize, double? BlackHoleFitness, double? EventHorizonRadius);
