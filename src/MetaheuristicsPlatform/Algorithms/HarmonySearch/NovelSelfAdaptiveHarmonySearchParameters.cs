using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class NovelSelfAdaptiveHarmonySearchParameters : IMetaheuristicParameters
{
    public int HarmonyMemorySize { get; init; } = 10;
    public int MaximumImprovisations { get; init; } = 50000;
    public double FitnessStandardDeviationThreshold { get; init; } = 0.0001;

    // HMCR is dimension-derived in NSHS; this property is a state-report fallback.
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.5;
    public double ReportedPitchAdjustmentRate => 0.0;
    public double ReportedPitchAdjustmentBandwidth => FitnessStandardDeviationThreshold;

    public static double GetHarmonyMemoryConsiderationRate(int dimension)
    {
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dimension));
        }
        return 1.0 - (1.0 / (dimension + 1.0));
    }

    public void Validate()
    {
        if (HarmonyMemorySize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(HarmonyMemorySize));
        }
        if (MaximumImprovisations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumImprovisations));
        }
        if (!double.IsFinite(HarmonyMemoryConsiderationRate) ||
            HarmonyMemoryConsiderationRate < 0.0 ||
            HarmonyMemoryConsiderationRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(HarmonyMemoryConsiderationRate));
        }
        if (!double.IsFinite(FitnessStandardDeviationThreshold) ||
            FitnessStandardDeviationThreshold <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FitnessStandardDeviationThreshold));
        }

    }
}
