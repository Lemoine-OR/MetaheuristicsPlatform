namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Immutable generation-level context supplied to adaptation policies.
/// </summary>
public readonly record struct DeGenerationAdaptationContext(
    int Generation,
    int ActivePopulationSize,
    long FunctionEvaluations,
    long? MaximumFunctionEvaluations);