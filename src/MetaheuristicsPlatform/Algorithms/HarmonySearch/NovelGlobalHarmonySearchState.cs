namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Observable state of Novel Global Harmony Search.
/// </summary>
public readonly record struct NovelGlobalHarmonySearchState(
    int Iteration,
    HarmonySearchPhase Phase,
    int HarmonyMemorySize,
    int TotalImprovisations,
    double MutationProbability,
    int MutatedCoordinateCount,
    bool UnconditionallyReplacedWorstHarmony,
    bool CandidateWasStrictlyBetterThanReplacedWorst,
    double? CandidateFitness,
    double? ReplacedWorstFitness,
    double? MemoryBestFitness,
    double? MemoryWorstFitness);
