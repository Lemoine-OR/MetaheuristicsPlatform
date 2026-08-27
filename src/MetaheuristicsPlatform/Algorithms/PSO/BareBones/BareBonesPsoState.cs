namespace MetaheuristicsPlatform.Algorithms.PSO.BareBones;

internal sealed record BareBonesPsoState(
    int CompletedIterations,
    int SwarmSize,
    double? GlobalBestFitness);
