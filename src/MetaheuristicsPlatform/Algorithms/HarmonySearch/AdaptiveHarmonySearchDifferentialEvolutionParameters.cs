using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class AdaptiveHarmonySearchDifferentialEvolutionParameters : IMetaheuristicParameters
{
    public int HarmonyMemorySize { get; init; } = 90;
    public int MaximumImprovisations { get; init; } = 100000;
    public double HarmonyMemoryConsiderationRate { get; init; } = 0.99;

    public int MinimumHarmonyMemorySize { get; init; } = 5;
    public int MaximumHarmonyMemorySizePerDimension { get; init; } = 18;
    public int MaximumFunctionEvaluationsPerDimension { get; init; } = 10000;
    public int LearningPeriod { get; init; } = 100;
    public double PitchAdjustmentBandwidth { get; init; } = 0.01;
    public double InitialPitchAdjustmentRateMean { get; init; } = 0.5;
    public double InitialScaleFactorMean { get; init; } = 0.5;

    public double ReportedPitchAdjustmentRate => InitialPitchAdjustmentRateMean;
    public double ReportedPitchAdjustmentBandwidth => PitchAdjustmentBandwidth;

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
        if (MinimumHarmonyMemorySize < 5)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumHarmonyMemorySize));
        }
        if (MaximumHarmonyMemorySizePerDimension <= 0 ||
            MaximumFunctionEvaluationsPerDimension <= 0 ||
            LearningPeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumHarmonyMemorySizePerDimension));
        }
        if (!double.IsFinite(PitchAdjustmentBandwidth) ||
            PitchAdjustmentBandwidth < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(PitchAdjustmentBandwidth));
        }

    }
}
