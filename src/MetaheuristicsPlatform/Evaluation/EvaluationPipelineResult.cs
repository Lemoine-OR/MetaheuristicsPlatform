namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Allocation-free summary of one pipeline evaluation.
/// </summary>
public readonly record struct EvaluationPipelineResult<TSolution>(
    double Fitness,
    TSolution Solution,
    bool WasRepaired,
    bool WasImproved,
    bool FeedbackApplied);