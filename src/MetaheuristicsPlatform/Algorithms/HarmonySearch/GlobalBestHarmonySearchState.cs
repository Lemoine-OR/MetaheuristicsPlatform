namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Observable state of Global-best Harmony Search.
/// </summary>
/// <remarks>
/// GHS has no bandwidth parameter. A dedicated state type avoids representing an absent
/// scientific parameter as a numerical zero in the public runtime state.
/// </remarks>
public readonly record struct GlobalBestHarmonySearchState(
    int Iteration,
    HarmonySearchPhase Phase,
    int HarmonyMemorySize,
    int TotalImprovisations,
    bool ReplacedWorstHarmony,
    double HarmonyMemoryConsiderationRate,
    double PitchAdjustmentRate,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);
