using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class DifferentialHarmonySearchParameters : IMetaheuristicParameters
{
    public int HarmonyMemorySize { get; init; } = 10;
    public int MaximumImprovisations { get; init; } = 1000;
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.9;

    // DHS replaces pitch adjustment entirely by differential mutation.
    public double ReportedPitchAdjustmentRate => 1.0;
    public double ReportedPitchAdjustmentBandwidth => 0.0;

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

    }
}
