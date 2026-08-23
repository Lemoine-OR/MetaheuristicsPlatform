using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of Self-Adaptive Global-best Harmony Search (SGHS)
/// following Pan, Suganthan, Tasgetiren and Liang (2010).
/// </summary>
public sealed class SelfAdaptiveGlobalBestHarmonySearchParameters :
    IMetaheuristicParameters
{
    public const double HarmonyMemoryConsiderationRateMinimum = 0.9;
    public const double HarmonyMemoryConsiderationRateMaximum = 1.0;
    public const double PitchAdjustmentRateMinimum = 0.0;
    public const double PitchAdjustmentRateMaximum = 1.0;
    public const double HarmonyMemoryConsiderationRateStandardDeviation = 0.01;
    public const double PitchAdjustmentRateStandardDeviation = 0.05;

    /// <summary>Gets Harmony Memory Size (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 5;

    /// <summary>Gets the maximum number of completed improvisations (NI).</summary>
    public int MaximumImprovisations { get; init; } = 1000;

    /// <summary>Gets the initial mean HMCRm.</summary>
    public double InitialMeanHarmonyMemoryConsiderationRate { get; init; } = 0.98;

    /// <summary>Gets the initial mean PARm.</summary>
    public double InitialMeanPitchAdjustmentRate { get; init; } = 0.9;

    /// <summary>Gets the learning period LP.</summary>
    public int LearningPeriod { get; init; } = 100;

    /// <summary>
    /// Gets BWmin in absolute coordinate units.
    /// Pan et al.'s experimental setting is 0.0005.
    /// </summary>
    public double MinimumPitchAdjustmentBandwidth { get; init; } = 0.0005;

    /// <summary>
    /// Gets the fraction used to lift the paper's BWmax=(UB-LB)/10 prescription
    /// coordinate-wise to a bounded box. The canonical default is 0.1.
    /// </summary>
    public double MaximumPitchAdjustmentBandwidthFractionOfRange { get; init; } = 0.1;

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

        if (LearningPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(LearningPeriod),
                LearningPeriod,
                "Learning period must be positive.");
        }

        ValidateInRange(
            InitialMeanHarmonyMemoryConsiderationRate,
            HarmonyMemoryConsiderationRateMinimum,
            HarmonyMemoryConsiderationRateMaximum,
            nameof(InitialMeanHarmonyMemoryConsiderationRate));

        ValidateInRange(
            InitialMeanPitchAdjustmentRate,
            PitchAdjustmentRateMinimum,
            PitchAdjustmentRateMaximum,
            nameof(InitialMeanPitchAdjustmentRate));

        if (!double.IsFinite(MinimumPitchAdjustmentBandwidth) ||
            MinimumPitchAdjustmentBandwidth <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumPitchAdjustmentBandwidth),
                MinimumPitchAdjustmentBandwidth,
                "BWmin must be finite and strictly positive.");
        }

        if (!double.IsFinite(MaximumPitchAdjustmentBandwidthFractionOfRange) ||
            MaximumPitchAdjustmentBandwidthFractionOfRange <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumPitchAdjustmentBandwidthFractionOfRange),
                MaximumPitchAdjustmentBandwidthFractionOfRange,
                "The BWmax range fraction must be finite and strictly positive.");
        }
    }

    /// <summary>
    /// Returns the SGHS bandwidth for one coordinate.
    /// </summary>
    /// <remarks>
    /// The paper uses BWmax=(UB-LB)/10 in its continuous experiments. The platform
    /// generalizes this prescription coordinate-wise for heterogeneous boxes.
    /// </remarks>
    public double GetPitchAdjustmentBandwidth(
        int generation,
        double coordinateSpan)
    {
        Validate();

        if (generation < 1 ||
            generation > MaximumImprovisations)
        {
            throw new ArgumentOutOfRangeException(
                nameof(generation),
                generation,
                $"Generation must be in [1,{MaximumImprovisations}].");
        }

        if (!double.IsFinite(coordinateSpan) ||
            coordinateSpan <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(coordinateSpan),
                coordinateSpan,
                "Coordinate span must be finite and strictly positive.");
        }

        double maximumBandwidth =
            MaximumPitchAdjustmentBandwidthFractionOfRange *
            coordinateSpan;

        if (maximumBandwidth <
            MinimumPitchAdjustmentBandwidth)
        {
            throw new ArgumentException(
                "The configured SGHS BWmin exceeds the coordinate-wise BWmax. " +
                "Lower BWmin or increase the BWmax fraction.");
        }

        if ((2L * generation) >= MaximumImprovisations)
        {
            return MinimumPitchAdjustmentBandwidth;
        }

        return
            maximumBandwidth -
            (((maximumBandwidth -
               MinimumPitchAdjustmentBandwidth) /
              MaximumImprovisations) *
             (2.0 * generation));
    }

    private static void ValidateInRange(
        double value,
        double minimum,
        double maximum,
        string parameterName)
    {
        if (!double.IsFinite(value) ||
            value < minimum ||
            value > maximum)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Value must be finite and in [{minimum},{maximum}].");
        }
    }
}
