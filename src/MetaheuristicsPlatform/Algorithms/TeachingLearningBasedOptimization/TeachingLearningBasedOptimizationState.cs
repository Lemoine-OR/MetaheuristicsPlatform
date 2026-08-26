namespace MetaheuristicsPlatform.Algorithms.TeachingLearningBasedOptimization;

public enum TeachingLearningBasedOptimizationPhase { Initialization = 0, Teacher = 1, Learner = 2, CompletedIteration = 3 }

public readonly record struct TeachingLearningBasedOptimizationState(int Iteration, TeachingLearningBasedOptimizationPhase Phase, int PopulationSize, double? BestFitness);
