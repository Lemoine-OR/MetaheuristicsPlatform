namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Observable state of iteration-dependent Advanced PSF-HS.
/// </summary>
public readonly record struct AdvancedParameterSettingFreeHarmonySearchIterationState(
    int Iteration,
    HarmonySearchPhase Phase,
    int HarmonyMemorySize,
    int TotalImprovisations,
    double HarmonyMemoryConsiderationRate,
    double PitchAdjustmentRate,
    double MinimumPitchAdjustmentBandwidth,
    double MaximumPitchAdjustmentBandwidth,
    bool ReplacedWorstHarmony,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);
