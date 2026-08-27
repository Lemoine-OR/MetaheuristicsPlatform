namespace MetaheuristicsPlatform.Algorithms.PSO.Standard2007;

internal sealed record StandardPso2007State(
    int CompletedIterations,
    int SwarmSize,
    int ExpectedInformerCount,
    double? GlobalBestFitness);
