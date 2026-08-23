namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Observable state of object-dependent Advanced PSF-HS.
/// </summary>
public readonly record struct AdvancedParameterSettingFreeHarmonySearchObjectState(
    int Iteration,
    HarmonySearchPhase Phase,
    bool IsRehearsal,
    int HarmonyMemorySize,
    int TotalImprovisations,
    int CompletedAdaptiveBandwidthBlocks,
    double TargetObjective,
    double HarmonyMemoryConsiderationRate,
    double PitchAdjustmentRate,
    double CurrentHarmonyMemoryMean,
    double? LossStart,
    double CurrentBandwidthFractionOfRange,
    bool ReplacedWorstHarmony,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);
