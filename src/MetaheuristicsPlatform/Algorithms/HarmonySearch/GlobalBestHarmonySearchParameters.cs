using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Parameters of Global-best Harmony Search (GHS) following Omran and Mahdavi (2008).
/// </summary>
public sealed class GlobalBestHarmonySearchParameters : IMetaheuristicParameters
{
    /// <summary>Gets the Harmony Memory Size (HMS).</summary>
    public int HarmonyMemorySize { get; init; } = 5;

    /// <summary>Gets the maximum number of completed improvisations (NI).</summary>
    public int MaximumImprovisations { get; init; } = 1000;

    /// <summary>Gets the fixed Harmony Memory Considering Rate (HMCR).</summary>
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.9;

    /// <summary>Gets PAR_min.</summary>
    public double MinimumPitchAdjustmentRate { get; init; } = 0.01;

    /// <summary>Gets PAR_max.</summary>
    public double MaximumPitchAdjustmentRate { get; init; } = 0.99;

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
    }

    /// <summary>
    /// Returns the IHS/GHS dynamic schedule
    /// PAR(t) = PAR_min + (PAR_max - PAR_min) * t / NI.
    /// The platform maps configured improvisations to t = 1,...,NI.
    /// </summary>
    public double GetPitchAdjustmentRate(int generation)
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

        double fraction =
            (double)generation /
            MaximumImprovisations;

        return
            MinimumPitchAdjustmentRate +
            ((MaximumPitchAdjustmentRate -
              MinimumPitchAdjustmentRate) *
             fraction);
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
