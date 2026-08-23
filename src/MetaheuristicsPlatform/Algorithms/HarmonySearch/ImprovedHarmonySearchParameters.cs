using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of the Improved Harmony Search algorithm of
/// Mahdavi, Fesanghary and Damangir (2007).
/// </summary>
public sealed class ImprovedHarmonySearchParameters : IMetaheuristicParameters
{
    /// <summary>Gets the number of harmonies stored in Harmony Memory (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 5;

    /// <summary>Gets the maximum number of completed improvisations (NI).</summary>
    public int MaximumImprovisations { get; init; } = 1000;

    /// <summary>Gets the fixed Harmony Memory Considering Rate (HMCR).</summary>
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.9;

    /// <summary>Gets PAR_min.</summary>
    public double MinimumPitchAdjustmentRate { get; init; } = 0.01;

    /// <summary>Gets PAR_max.</summary>
    public double MaximumPitchAdjustmentRate { get; init; } = 0.99;

    /// <summary>Gets bw_min in absolute coordinate units.</summary>
    public double MinimumPitchAdjustmentBandwidth { get; init; } = 0.0001;

    /// <summary>Gets bw_max in absolute coordinate units.</summary>
    public double MaximumPitchAdjustmentBandwidth { get; init; } = 1.0;

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
            MinimumPitchAdjustmentRate,
            nameof(MinimumPitchAdjustmentRate));

        ValidateProbability(
            MaximumPitchAdjustmentRate,
            nameof(MaximumPitchAdjustmentRate));

        if (MinimumPitchAdjustmentRate >
            MaximumPitchAdjustmentRate)
        {
            throw new ArgumentException(
                "PAR_min must not exceed PAR_max.");
        }

        if (!double.IsFinite(MinimumPitchAdjustmentBandwidth) ||
            MinimumPitchAdjustmentBandwidth <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumPitchAdjustmentBandwidth),
                MinimumPitchAdjustmentBandwidth,
                "bw_min must be finite and strictly positive.");
        }

        if (!double.IsFinite(MaximumPitchAdjustmentBandwidth) ||
            MaximumPitchAdjustmentBandwidth <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumPitchAdjustmentBandwidth),
                MaximumPitchAdjustmentBandwidth,
                "bw_max must be finite and strictly positive.");
        }

        if (MinimumPitchAdjustmentBandwidth >
            MaximumPitchAdjustmentBandwidth)
        {
            throw new ArgumentException(
                "bw_min must not exceed bw_max.");
        }
    }

    /// <summary>
    /// Returns PAR(t) = PAR_min + (PAR_max - PAR_min) * t / NI.
    /// The platform maps completed improvisations to t = 1,...,NI.
    /// </summary>
    public double GetPitchAdjustmentRate(int generation)
    {
        Validate();
        ValidateGeneration(generation);

        double fraction =
            (double)generation /
            MaximumImprovisations;

        return
            MinimumPitchAdjustmentRate +
            ((MaximumPitchAdjustmentRate -
              MinimumPitchAdjustmentRate) *
             fraction);
    }

    /// <summary>
    /// Returns bw(t) = bw_max * exp((t/NI) * ln(bw_min/bw_max)).
    /// </summary>
    public double GetPitchAdjustmentBandwidth(int generation)
    {
        Validate();
        ValidateGeneration(generation);

        double fraction =
            (double)generation /
            MaximumImprovisations;

        return
            MaximumPitchAdjustmentBandwidth *
            Math.Exp(
                fraction *
                Math.Log(
                    MinimumPitchAdjustmentBandwidth /
                    MaximumPitchAdjustmentBandwidth));
    }

    private void ValidateGeneration(int generation)
    {
        if (generation < 1 ||
            generation > MaximumImprovisations)
        {
            throw new ArgumentOutOfRangeException(
                nameof(generation),
                generation,
                $"Generation must be in [1,{MaximumImprovisations}].");
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
