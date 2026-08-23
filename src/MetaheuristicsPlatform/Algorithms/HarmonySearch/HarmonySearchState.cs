namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public enum HarmonySearchPhase
{
    Initialization = 0,
    Improvisation = 1,
    CompletedImprovisation = 2
}

public readonly record struct HarmonySearchState(
    int Iteration,
    HarmonySearchPhase Phase,
    int HarmonyMemorySize,
    int TotalImprovisations,
    bool ReplacedWorstHarmony,
    double HarmonyMemoryConsiderationRate,
    double PitchAdjustmentRate,
    double PitchAdjustmentBandwidth,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);