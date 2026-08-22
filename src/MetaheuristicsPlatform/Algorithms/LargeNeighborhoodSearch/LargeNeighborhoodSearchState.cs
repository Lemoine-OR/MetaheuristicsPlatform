namespace MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;

/// <summary>Observable state of one Large Neighborhood Search run.</summary>
public readonly record struct LargeNeighborhoodSearchState(
    int IterationsCompleted,
    double CurrentObjective,
    double BestObjective,
    double LastCandidateObjective,
    int DestructionSize,
    long DestroyInvocations,
    long RepairInvocations,
    long AcceptedCandidates,
    long RejectedCandidates);
