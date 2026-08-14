namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Execution-relevant properties of a problem's candidate evaluation.
/// </summary>
public readonly record struct EvaluationCharacteristics(
    bool SupportsParallelEvaluation,
    EvaluationCostHint CostHint = EvaluationCostHint.Unknown,
    EvaluationVariabilityHint VariabilityHint = EvaluationVariabilityHint.Unknown);