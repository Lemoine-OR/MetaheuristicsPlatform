namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

internal sealed record ScientificCanonicalPsoState(
    int CompletedIterations,
    string Variant,
    int SwarmSize,
    double? BestPersonalFitness);
