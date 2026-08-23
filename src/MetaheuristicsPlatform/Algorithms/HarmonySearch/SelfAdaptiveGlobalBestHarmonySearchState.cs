namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Observable state of Self-Adaptive Global-best Harmony Search.
/// </summary>
public readonly record struct SelfAdaptiveGlobalBestHarmonySearchState(
    int Iteration,
    HarmonySearchPhase Phase,
    int HarmonyMemorySize,
    int TotalImprovisations,
    bool ReplacedWorstHarmony,
    double HarmonyMemoryConsiderationRate,
    double PitchAdjustmentRate,
    double MeanHarmonyMemoryConsiderationRate,
    double MeanPitchAdjustmentRate,
    int LearningPeriodPosition,
    int SuccessfulSamplesInCurrentLearningPeriod,
    int LearningUpdates,
    int LastCompletedLearningPeriodSuccessfulSamples,
    double MinimumCurrentBandwidth,
    double MaximumCurrentBandwidth,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);
