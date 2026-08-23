using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of the canonical Harmony Search algorithm of Geem, Kim and Loganathan.
/// </summary>
public sealed class HarmonySearchParameters : IMetaheuristicParameters
{
    /// <summary>Gets the number of harmonies stored in Harmony Memory (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 20;

    /// <summary>Gets the maximum number of completed improvisations.</summary>
    public int MaximumImprovisations { get; init; } = 1000;

    /// <summary>Gets the Harmony Memory Considering Rate (HMCR).</summary>
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.9;

    /// <summary>Gets the Pitch Adjusting Rate (PAR).</summary>
    public double PitchAdjustmentRate { get; init; } = 0.3;

    /// <summary>
    /// Gets the absolute pitch-adjustment bandwidth bw in search-space coordinate units.
    /// </summary>
    public double PitchAdjustmentBandwidth { get; init; } = 0.01;

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

        ValidateProbability(
            HarmonyMemoryConsiderationRate,
            nameof(HarmonyMemoryConsiderationRate));

        ValidateProbability(
            PitchAdjustmentRate,
            nameof(PitchAdjustmentRate));

        if (!double.IsFinite(PitchAdjustmentBandwidth) ||
            PitchAdjustmentBandwidth < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PitchAdjustmentBandwidth),
                PitchAdjustmentBandwidth,
                "Pitch-adjustment bandwidth must be finite and non-negative.");
        }
    }

    private static void ValidateProbability(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) ||
            value < 0.0 ||
            value > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Probability parameters must be finite and in [0,1].");
        }
    }
}