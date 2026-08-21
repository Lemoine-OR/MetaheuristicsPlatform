namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>
/// Stable scientific component identifiers for Advanced Scatter Search.
/// These are component IDs, not additional public algorithm IDs.
/// </summary>
public static class ScatterSearchComponentIds
{
    public const string DynamicRefSetRefresh =
        "ss.refset.update.dynamic-refresh";

    public const string TwoTierRefSetUpdate =
        "ss.refset.update.two-tier";

    public const string MaxMinRefSetRebuild =
        "ss.refset.rebuild.max-min";

    public const string MinimumDiversity =
        "ss.diversity.minimum-distance";

    public const string GloverSubsetTypesOneToFour =
        "ss.subsets.glover-types-1-4";

    public const string ThreeTierGoodGenerators =
        "ss.refset.update.three-tier-good-generators";

    public const string HashingDuplicateControl =
        "ss.diversity.hashing";

    public const string VariableCardinalityCombination =
        "ss.combination.variable-cardinality";

    public const string BinaryCombination =
        "ss.combination.binary";

    public const string ExplicitEvaluatedSolutionMemory =
        "ss.memory.explicit-evaluated-solutions";

    public const string DeepPathRelinkingIntegration =
        "ss.path-relinking.deep-integration";
}
