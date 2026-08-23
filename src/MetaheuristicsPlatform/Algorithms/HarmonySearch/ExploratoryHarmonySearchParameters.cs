using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class ExploratoryHarmonySearchParameters : IMetaheuristicParameters
{
    public int HarmonyMemorySize { get; init; } = 10;
    public int MaximumImprovisations { get; init; } = 1000;
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.99;
    public double PitchAdjustmentRate { get; init; } = 0.33;
    public double StandardDeviationMultiplier { get; init; } = 1.17;

    public double ReportedPitchAdjustmentRate => PitchAdjustmentRate;
    public double ReportedPitchAdjustmentBandwidth => StandardDeviationMultiplier;

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
        if (!double.IsFinite(PitchAdjustmentRate) ||
            PitchAdjustmentRate < 0.0 || PitchAdjustmentRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(PitchAdjustmentRate));
        }
        if (!double.IsFinite(StandardDeviationMultiplier) ||
            StandardDeviationMultiplier < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(StandardDeviationMultiplier));
        }

    }
}
