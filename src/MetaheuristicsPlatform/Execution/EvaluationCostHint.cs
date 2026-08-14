namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Coarse, problem-provided indication of one candidate evaluation cost.
/// It is a scheduling hint, not a semantic property of the objective.
/// </summary>
public enum EvaluationCostHint
{
    Unknown = 0,
    Trivial = 1,
    Light = 2,
    Medium = 3,
    Heavy = 4,
    VeryHeavy = 5
}