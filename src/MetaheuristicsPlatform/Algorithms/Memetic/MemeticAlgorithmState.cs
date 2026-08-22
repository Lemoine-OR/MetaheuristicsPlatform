namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>Observable state of the GA-backed memetic execution.</summary>
public readonly record struct MemeticAlgorithmState(
    int Generation,
    int PopulationCount,
    long OffspringEvaluated,
    long ParentSelections,
    long CrossoverEvents,
    long MutationEvents,
    int EliteCount,
    long LocalSearchInvocations,
    long SuccessfulLocalSearches,
    long AcceptedLocalSearchMoves,
    double CumulativeLocalSearchGain,
    int ConsecutiveNonImprovingGenerations,
    double LastLocalSearchProbability,
    MemeticLearningMode LearningMode)
{
    public double LocalSearchSuccessRate =>
        LocalSearchInvocations == 0
            ? 0.0
            : (double)SuccessfulLocalSearches /
              LocalSearchInvocations;
}
