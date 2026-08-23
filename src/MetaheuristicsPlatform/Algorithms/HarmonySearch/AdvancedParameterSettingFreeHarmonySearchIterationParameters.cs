using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of the iteration-dependent Advanced Parameter-Setting-Free
/// Harmony Search scheme of Jeong, Park, Geem and Sim (2020).
/// </summary>
public sealed class AdvancedParameterSettingFreeHarmonySearchIterationParameters :
    IMetaheuristicParameters
{
    /// <summary>Gets Harmony Memory Size (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 50;

    /// <summary>Gets maximum improvisations NI used by the published HMCR schedule.</summary>
    public int MaximumImprovisations { get; init; } = 20_000;

    /// <summary>
    /// Gets fixed pitch-adjustment bandwidth as a fraction of each coordinate range.
    /// The 2020 paper explicitly states that object-dependent adaptive bandwidth is
    /// unavailable for the iteration-dependent HMCR scheme.
    /// </summary>
    public double PitchAdjustmentBandwidthFractionOfRange { get; init; } = 0.001;

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

        if (!double.IsFinite(PitchAdjustmentBandwidthFractionOfRange) ||
            PitchAdjustmentBandwidthFractionOfRange < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PitchAdjustmentBandwidthFractionOfRange),
                PitchAdjustmentBandwidthFractionOfRange,
                "Bandwidth fraction must be finite and nonnegative.");
        }
    }

    /// <summary>
    /// Gets the published iteration-dependent HMCR for one-based improvisation t.
    /// </summary>
    public double GetHarmonyMemoryConsiderationRate(
        int improvisation,
        int dimension)
    {
        ValidateScheduleArguments(
            improvisation,
            dimension);

        if (dimension == 1)
        {
            // Equation (5) contains log(1)=0. The right-hand limit D -> 1+
            // sends -5/log(D) to -infinity, hence sigmoid(.) -> 0 and HMCR -> 0.5.
            return 0.5;
        }

        double argument =
            (10.0 * improvisation /
             MaximumImprovisations) -
            (5.0 / Math.Log(dimension));

        return
            0.5 +
            (0.5 * Sigmoid(argument));
    }

    /// <summary>Gets the published PAR from current HMCR and dimension.</summary>
    public static double GetPitchAdjustmentRate(
        double harmonyMemoryConsiderationRate,
        int dimension)
    {
        if (!double.IsFinite(harmonyMemoryConsiderationRate) ||
            harmonyMemoryConsiderationRate < 0.0 ||
            harmonyMemoryConsiderationRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(harmonyMemoryConsiderationRate));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        return
            harmonyMemoryConsiderationRate *
            Sigmoid(
                (4.0 / dimension) -
                2.0);
    }

    private void ValidateScheduleArguments(
        int improvisation,
        int dimension)
    {
        if (improvisation <= 0 ||
            improvisation > MaximumImprovisations)
        {
            throw new ArgumentOutOfRangeException(
                nameof(improvisation));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }
    }

    private static double Sigmoid(
        double value) =>
        1.0 /
        (1.0 +
         Math.Exp(-value));
}
