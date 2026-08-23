using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class ImprovedHarmonySearchDifferentialMutationParameters : IMetaheuristicParameters
{
    public int HarmonyMemorySize { get; init; } = 10;
    public int MaximumImprovisations { get; init; } = 10000;
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.8;

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
        if (HarmonyMemorySize < 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(HarmonyMemorySize),
                "IHSDE requires at least three harmonies.");
        }

    }
}
