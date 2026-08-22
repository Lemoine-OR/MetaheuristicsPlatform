namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Restart regime exposed through the common algorithm state.</summary>
public enum RestartCmaEsRegime
{
    Initial = 0,
    LargePopulation = 1,
    SmallPopulation = 2
}

/// <summary>Observable state shared by IPOP-CMA-ES and BIPOP-CMA-ES.</summary>
public sealed record RestartCmaEsState(
    int RestartIndex,
    RestartCmaEsRegime Regime,
    int GenerationInRestart,
    int PopulationSize,
    int ParentCount,
    double StepSize,
    double ConditionNumberEstimate,
    long LargePopulationEvaluationBudget,
    long SmallPopulationEvaluationBudget,
    double? GenerationBestFitness);
