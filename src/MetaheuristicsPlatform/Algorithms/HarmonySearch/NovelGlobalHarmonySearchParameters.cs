using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of Novel Global Harmony Search (NGHS) following
/// Zou, Gao, Wu and Li (2010).
/// </summary>
public sealed class NovelGlobalHarmonySearchParameters :
    IMetaheuristicParameters
{
    /// <summary>Gets Harmony Memory Size (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 5;

    /// <summary>Gets the maximum number of completed improvisations (NI).</summary>
    public int MaximumImprovisations { get; init; } = 1000;

    /// <summary>
    /// Gets the coordinate-wise genetic mutation probability p_m.
    /// The default 0.005 is the canonical continuous-optimization setting
    /// repeatedly reported for NGHS.
    /// </summary>
    public double MutationProbability { get; init; } = 0.005;

    public void Validate()
    {
        if (HarmonyMemorySize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(HarmonyMemorySize),
                HarmonyMemorySize,
                "Harmony memory size must be positive.");
        }

        if (MaximumImprovisations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumImprovisations),
                MaximumImprovisations,
                "Maximum improvisations must be positive.");
        }

        if (!double.IsFinite(MutationProbability) ||
            MutationProbability < 0.0 ||
            MutationProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MutationProbability),
                MutationProbability,
                "Mutation probability must be finite and in [0,1].");
        }
    }
}
