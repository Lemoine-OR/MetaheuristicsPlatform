namespace MetaheuristicsPlatform.Algorithms.PSO.Cooperative;

internal sealed record CooperativePsoState(
    int CompletedIterations,
    int SubswarmCount,
    int SubswarmSize,
    double? ContextFitness);
