namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Determines whether and how an improved phenotype affects the candidate representation.
/// </summary>
public enum ImprovementFeedbackMode
{
    /// <summary>No improvement stage is executed.</summary>
    None = 0,

    /// <summary>
    /// Improvement affects evaluated fitness but does not replace/update the candidate.
    /// </summary>
    Baldwinian = 1,

    /// <summary>
    /// Improvement affects evaluated fitness and is projected back into the candidate.
    /// </summary>
    Lamarckian = 2
}