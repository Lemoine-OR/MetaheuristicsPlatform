namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Operation type recorded by the PSF-HS Operation Type Matrix (OTM).
/// </summary>
public enum ParameterSettingFreeHarmonySearchOperationType
{
    RandomSelection = 0,
    MemoryConsideration = 1,
    PitchAdjustment = 2
}

/// <summary>
/// Execution stage of Parameter-Setting-Free Harmony Search.
/// </summary>
public enum ParameterSettingFreeHarmonySearchStage
{
    RandomTuning = 0,
    Rehearsal = 1,
    Performance = 2
}

/// <summary>
/// Observable state of Parameter-Setting-Free Harmony Search.
/// </summary>
public readonly record struct ParameterSettingFreeHarmonySearchState(
    int Iteration,
    HarmonySearchPhase Phase,
    ParameterSettingFreeHarmonySearchStage Stage,
    int HarmonyMemorySize,
    int TotalImprovisations,
    int RehearsalImprovisations,
    int RandomOperationCount,
    int MemoryOperationCount,
    int PitchOperationCount,
    double MinimumHarmonyMemoryConsiderationRate,
    double MaximumHarmonyMemoryConsiderationRate,
    double MinimumPitchAdjustmentRate,
    double MaximumPitchAdjustmentRate,
    bool ReplacedWorstHarmony,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);
