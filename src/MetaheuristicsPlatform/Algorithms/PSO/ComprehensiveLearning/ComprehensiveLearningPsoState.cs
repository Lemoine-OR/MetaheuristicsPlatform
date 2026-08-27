namespace MetaheuristicsPlatform.Algorithms.PSO.ComprehensiveLearning;

internal sealed record ComprehensiveLearningPsoState(
    int CompletedIterations,
    int SwarmSize,
    double? BestPersonalFitness);
