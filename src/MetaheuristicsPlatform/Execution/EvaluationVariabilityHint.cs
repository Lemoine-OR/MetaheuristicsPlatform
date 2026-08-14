namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Expected variation of evaluation time between candidates.
/// </summary>
public enum EvaluationVariabilityHint
{
    Unknown = 0,
    Uniform = 1,
    Moderate = 2,
    High = 3
}