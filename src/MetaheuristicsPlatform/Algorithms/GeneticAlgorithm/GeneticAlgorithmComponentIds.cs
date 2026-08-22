namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Stable scientific component identifiers for advanced Genetic Algorithm operators.
/// These are component IDs, not additional public algorithm IDs.
/// </summary>
public static class GeneticAlgorithmComponentIds
{
    public const string TournamentSelection =
        "ga.selection.tournament";

    public const string TruncationSelection =
        "ga.selection.truncation";

    public const string LinearRankingSelection =
        "ga.selection.linear-ranking";

    public const string ExponentialRankingSelection =
        "ga.selection.exponential-ranking";

    public const string ExplicitFitnessProportionateSelection =
        "ga.selection.fitness-proportionate-explicit-weights";

    public const string OnePointCrossover =
        "ga.crossover.one-point";

    public const string TwoPointCrossover =
        "ga.crossover.two-point";

    public const string UniformCrossover =
        "ga.crossover.uniform";

    public const string PartiallyMappedCrossover =
        "ga.crossover.pmx";

    public const string OrderCrossover =
        "ga.crossover.ox1";

    public const string BoundedSimulatedBinaryCrossover =
        "ga.crossover.sbx-bounded";

    public const string BitFlipMutation =
        "ga.mutation.bit-flip";

    public const string IntegerRandomResetMutation =
        "ga.mutation.integer-random-reset";

    public const string SwapMutation =
        "ga.mutation.swap";

    public const string InversionMutation =
        "ga.mutation.inversion";

    public const string BoundedGaussianMutation =
        "ga.mutation.gaussian-bounded";

    public const string BoundedPolynomialMutation =
        "ga.mutation.polynomial-bounded";

    public const string GenerationalElitistReplacement =
        "ga.replacement.generational-elitist";

    public const string SteadyStateReplacement =
        "ga.replacement.steady-state";
}
