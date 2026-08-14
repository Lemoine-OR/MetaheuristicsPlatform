namespace MetaheuristicsPlatform.Evaluation.Caching;

/// <summary>
/// Complete cache-owned outcome of one problem evaluation.
/// </summary>
public readonly record struct EvaluationCacheEntry<TCandidate, TSolution>(
    double Fitness,
    TSolution SolutionSnapshot,
    TCandidate CandidateSnapshot,
    bool HasCandidateSnapshot,
    bool WasRepaired,
    bool WasImproved,
    bool FeedbackApplied);