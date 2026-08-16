namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Allocation-free cursor over the active Guided Local Search features of one solution.
/// Active features must be unique within one enumeration.
/// </summary>
public interface IGuidedLocalSearchFeatureEnumerator<TFeature>
{
    /// <summary>Moves to the next active feature.</summary>
    bool MoveNext(out TFeature feature);
}

/// <summary>
/// Domain-owned feature model used by Guided Local Search.
/// A feature is active when returned by <see cref="GetEnumerator"/> for the current solution.
/// </summary>
public interface IGuidedLocalSearchFeatureModel<
    TSolution,
    TFeature,
    TFeatureEnumerator>
    where TFeature : notnull
    where TFeatureEnumerator : struct, IGuidedLocalSearchFeatureEnumerator<TFeature>
{
    /// <summary>Returns an allocation-free cursor over the unique active features.</summary>
    TFeatureEnumerator GetEnumerator(in TSolution solution);

    /// <summary>
    /// Returns the non-negative feature cost used by the canonical GLS utility
    /// c_i / (1 + p_i). The value must be finite.
    /// </summary>
    double GetFeatureCost(in TSolution solution, in TFeature feature);
}

/// <summary>
/// Optional exact fast path for the unscaled GLS penalty sum after applying a move.
/// </summary>
public interface IGuidedLocalSearchPenaltyDeltaEvaluator<
    TSolution,
    TMove,
    TFeature>
    where TFeature : notnull
{
    /// <summary>
    /// Attempts to compute the candidate value of
    /// sum_i p_i I_i(x) without applying the move or rescanning all active features.
    /// </summary>
    bool TryEvaluateCandidatePenaltySum(
        in TSolution solution,
        long currentPenaltySum,
        in TMove move,
        IReadOnlyDictionary<TFeature, int> penalties,
        out long candidatePenaltySum);
}

/// <summary>Observable state exposed to common stopping criteria during Guided Local Search.</summary>
public readonly record struct GuidedLocalSearchState(
    int PenaltyUpdates,
    long AcceptedMoves,
    int DistinctPenalizedFeatures,
    long TotalPenaltyIncrements,
    double CurrentObjective,
    double CurrentAugmentedObjective);
