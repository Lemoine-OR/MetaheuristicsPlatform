namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

internal sealed record FullyInformedPsoState(
    int CompletedIterations,
    int SwarmSize,
    double? BestPersonalFitness);
