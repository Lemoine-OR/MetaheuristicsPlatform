namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Selection result for one target.
/// Improvement must be non-negative and expressed in objective-quality units
/// independent of minimization/maximization direction.
/// </summary>
public readonly record struct DeSelectionFeedback(
    int TargetIndex,
    bool Accepted,
    double ParentFitness,
    double TrialFitness,
    double Improvement);