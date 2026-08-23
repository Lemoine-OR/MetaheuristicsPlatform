using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of the object-dependent Advanced Parameter-Setting-Free
/// Harmony Search scheme of Jeong, Park, Geem and Sim (2020).
/// </summary>
public sealed class AdvancedParameterSettingFreeHarmonySearchObjectParameters :
    IMetaheuristicParameters
{
    public int HarmonyMemorySize { get; init; } = 50;

    /// <summary>
    /// Safety ceiling supplied by the platform. The scientific Object-PSF
    /// termination quantity is TargetObjective.
    /// </summary>
    public int MaximumImprovisations { get; init; } = 20_000;

    /// <summary>
    /// Object value Loss_obj. Equation (7) is published for global minimization.
    /// </summary>
    public double TargetObjective { get; init; } = 0.0;

    /// <summary>
    /// Fixed rehearsal HMCR during the first HMS improvisations.
    /// The paper leaves this "specific value" underspecified; 0.5 is the
    /// platform default consistent with the paper's PSF rehearsal examples.
    /// </summary>
    public double RehearsalHarmonyMemoryConsiderationRate { get; init; } = 0.5;

    /// <summary>Fixed rehearsal PAR during the first HMS improvisations.</summary>
    public double RehearsalPitchAdjustmentRate { get; init; } = 0.5;

    /// <summary>
    /// Initial bandwidth before Equation (9) can be evaluated.
    /// 0.001 corresponds to the paper's typical 0.1% full-range HS bandwidth.
    /// </summary>
    public double InitialPitchAdjustmentBandwidthFractionOfRange { get; init; } = 0.001;

    public void Validate()
    {
        if (HarmonyMemorySize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(HarmonyMemorySize));
        }

        if (MaximumImprovisations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumImprovisations));
        }

        if (!double.IsFinite(TargetObjective))
        {
            throw new ArgumentOutOfRangeException(
                nameof(TargetObjective));
        }

        ValidateProbability(
            RehearsalHarmonyMemoryConsiderationRate,
            nameof(RehearsalHarmonyMemoryConsiderationRate));

        ValidateProbability(
            RehearsalPitchAdjustmentRate,
            nameof(RehearsalPitchAdjustmentRate));

        if (!double.IsFinite(InitialPitchAdjustmentBandwidthFractionOfRange) ||
            InitialPitchAdjustmentBandwidthFractionOfRange < 0.0 ||
            InitialPitchAdjustmentBandwidthFractionOfRange > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialPitchAdjustmentBandwidthFractionOfRange));
        }
    }

    public double GetObjectHarmonyMemoryConsiderationRate(
        double lossMean,
        double lossStart,
        int dimension)
    {
        if (!double.IsFinite(lossMean) ||
            !double.IsFinite(lossStart))
        {
            throw new ArgumentOutOfRangeException();
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        double denominator =
            lossStart -
            TargetObjective;

        if (!(denominator > 0.0) ||
            !double.IsFinite(denominator))
        {
            throw new InvalidOperationException(
                "Object PSF-HS requires Loss_start > Loss_obj while search continues.");
        }

        if (dimension == 1)
        {
            return 0.5;
        }

        double argument =
            10.0 -
            (10.0 *
             ((lossMean - TargetObjective) /
              denominator)) -
            (5.0 / Math.Log(dimension));

        return
            0.5 +
            (0.5 * Sigmoid(argument));
    }

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

    /// <summary>
    /// Equation (9) expressed as a fraction of U-L. This permits the exact
    /// scalar equation to be lifted coordinate-wise to heterogeneous boxes.
    /// </summary>
    public double GetAdaptiveBandwidthFraction(
        double previousBlockMean,
        double currentBlockMean,
        double lossStart)
    {
        if (!double.IsFinite(previousBlockMean) ||
            !double.IsFinite(currentBlockMean) ||
            !double.IsFinite(lossStart))
        {
            throw new ArgumentOutOfRangeException();
        }

        double denominator =
            lossStart -
            TargetObjective;

        if (!(denominator > 0.0) ||
            !double.IsFinite(denominator))
        {
            throw new InvalidOperationException(
                "Equation (9) requires Loss_start > Loss_obj.");
        }

        double candidate =
            (previousBlockMean -
             currentBlockMean) /
            denominator;

        if (candidate >= 0.0001)
        {
            return candidate;
        }

        return
            (1.0 -
             ((lossStart -
               currentBlockMean) /
              denominator)) *
            0.1;
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
                parameterName);
        }
    }

    private static double Sigmoid(
        double value) =>
        1.0 /
        (1.0 +
         Math.Exp(-value));
}
