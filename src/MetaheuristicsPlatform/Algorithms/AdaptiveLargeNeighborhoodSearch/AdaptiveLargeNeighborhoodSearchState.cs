namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public readonly record struct AdaptiveLargeNeighborhoodSearchState(
    int IterationsCompleted,
    int Segment,
    int IterationInSegment,
    double CurrentObjective,
    double BestObjective,
    double LastCandidateObjective,
    string? DestroyOperatorId,
    string? RepairOperatorId,
    double DestroyOperatorWeight,
    double RepairOperatorWeight,
    double LastReward,
    bool LastCandidateAccepted,
    bool LastCandidateNovel,
    long DestroyInvocations,
    long RepairInvocations,
    long AcceptedCandidates,
    long RejectedCandidates,
    long SegmentWeightUpdates);
