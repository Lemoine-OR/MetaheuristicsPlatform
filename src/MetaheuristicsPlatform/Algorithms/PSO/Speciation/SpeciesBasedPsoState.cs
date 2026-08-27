namespace MetaheuristicsPlatform.Algorithms.PSO.Speciation;

internal sealed record SpeciesBasedPsoState(
    int CompletedIterations,
    int SwarmSize,
    int SpeciesCount,
    double? GlobalBestFitness);
