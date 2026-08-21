namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>Algorithm-specific state exposed through the common OptimizationContext.</summary>
public sealed record ScatterSearchState(
    int IterationsCompleted,
    int DiversificationSolutionsEvaluated,
    int ReferenceSetSize,
    int NewReferenceSolutions,
    long SubsetsGenerated,
    long CombinedCandidatesEvaluated,
    long ReferenceSetUpdates,
    long ImprovementInvocations);
