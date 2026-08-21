using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>Parameters for the canonical Scatter Search foundation.</summary>
public sealed class ScatterSearchParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Number of diversified complete solutions generated before the initial RefSet is built.
    /// </summary>
    public int DiversificationPopulationSize { get; init; } = 100;

    /// <summary>Target number of reference solutions.</summary>
    public int ReferenceSetSize { get; init; } = 10;

    /// <summary>
    /// Number of initial RefSet positions reserved for the best solutions by objective value.
    /// The remaining positions are selected by max-min diversity.
    /// </summary>
    public int QualityReferenceSetSize { get; init; } = 5;

    /// <summary>
    /// Maximum number of subset-generation/combination/update rounds.
    /// A stable RefSet terminates the run earlier unless an enabled rebuilding
    /// method successfully refreshes the diversity tier.
    /// </summary>
    public int MaximumIterations { get; init; } = 100;

    /// <summary>
    /// Controls whether a newly admitted reference solution waits for the
    /// current subset schedule to finish or triggers an immediate refresh.
    /// RoundSnapshot preserves the v0.39.0 behavior.
    /// </summary>
    public ScatterSearchReferenceSetRefreshMode ReferenceSetRefreshMode { get; init; } =
        ScatterSearchReferenceSetRefreshMode.RoundSnapshot;

    /// <summary>
    /// Maximum number of partial RefSet rebuilds after stable rounds.
    /// Zero disables rebuilding and preserves the v0.39.0 lifecycle.
    /// </summary>
    public int MaximumReferenceSetRebuilds { get; init; } = 0;

    /// <summary>
    /// Number of fresh diversified complete solutions generated for one
    /// optional RefSet rebuilding attempt.
    /// </summary>
    public int RebuildDiversificationPopulationSize { get; init; } = 100;

    public void Validate()
    {
        if (DiversificationPopulationSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(DiversificationPopulationSize));

        if (ReferenceSetSize < 2)
            throw new ArgumentOutOfRangeException(nameof(ReferenceSetSize));

        if (DiversificationPopulationSize < ReferenceSetSize)
            throw new ArgumentOutOfRangeException(
                nameof(DiversificationPopulationSize),
                "The diversified population must be at least as large as the reference set.");

        if (QualityReferenceSetSize <= 0 ||
            QualityReferenceSetSize >= ReferenceSetSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(QualityReferenceSetSize),
                "The initial RefSet must reserve at least one position for quality and one for diversity.");
        }

        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));

        if (!Enum.IsDefined(ReferenceSetRefreshMode))
            throw new ArgumentOutOfRangeException(nameof(ReferenceSetRefreshMode));

        if (MaximumReferenceSetRebuilds < 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumReferenceSetRebuilds));

        if (RebuildDiversificationPopulationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RebuildDiversificationPopulationSize));
        }
    }
}
