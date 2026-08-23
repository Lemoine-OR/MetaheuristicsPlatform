using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of Parameter-Setting-Free Harmony Search (PSF-HS)
/// following Geem and Sim (2010).
/// </summary>
public sealed class ParameterSettingFreeHarmonySearchParameters :
    IMetaheuristicParameters
{
    /// <summary>Gets Harmony Memory Size (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 30;

    /// <summary>Gets the maximum number of completed improvisations.</summary>
    public int MaximumImprovisations { get; init; } = 3000;

    /// <summary>
    /// Gets the number of Harmony-Memory-sized rehearsal cycles.
    /// A value of 3 reproduces the commonly reported conventional PSF rehearsal.
    /// </summary>
    public int RehearsalMemoryCycles { get; init; } = 3;

    /// <summary>
    /// Gets the pitch-adjustment bandwidth as a fraction of each coordinate range.
    /// PSF-HS removes manual HMCR/PAR setting, not the problem-scale bandwidth.
    /// </summary>
    public double PitchAdjustmentBandwidthFractionOfRange { get; init; } = 0.001;

    public const double RehearsalHarmonyMemoryConsiderationRate = 0.5;
    public const double RehearsalPitchAdjustmentRate = 0.5;

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

        if (RehearsalMemoryCycles <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RehearsalMemoryCycles),
                RehearsalMemoryCycles,
                "Rehearsal memory cycles must be positive.");
        }

        if (!double.IsFinite(PitchAdjustmentBandwidthFractionOfRange) ||
            PitchAdjustmentBandwidthFractionOfRange < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PitchAdjustmentBandwidthFractionOfRange),
                PitchAdjustmentBandwidthFractionOfRange,
                "Pitch-adjustment bandwidth fraction must be finite and nonnegative.");
        }
    }

    public int GetRehearsalImprovisations()
    {
        Validate();

        long requested =
            (long)HarmonyMemorySize *
            RehearsalMemoryCycles;

        return (int)Math.Min(
            requested,
            MaximumImprovisations);
    }
}
