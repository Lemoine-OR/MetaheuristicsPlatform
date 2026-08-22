namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Observable state of one completed CMA-ES generation.</summary>
public readonly record struct CmaEsState(
    int Generation,
    int PopulationSize,
    int ParentCount,
    double StepSize,
    double ConditionNumberEstimate,
    double? GenerationBestFitness);
