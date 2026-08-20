namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

public readonly record struct IteratedGreedyState(
    int IterationsCompleted,
    double CurrentObjective,
    double BestObjective,
    double LastCandidateObjective,
    int DestructionSize,
    long AcceptedCandidates,
    long RejectedCandidates,
    long LocalSearchInvocations,
    long AcceptedLocalSearchMoves);
