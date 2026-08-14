namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public readonly record struct DePopulationSizeContext(
    int InitialPopulationSize,
    int CurrentPopulationSize,
    int MinimumPopulationSize,
    long FunctionEvaluations,
    long MaximumFunctionEvaluations);