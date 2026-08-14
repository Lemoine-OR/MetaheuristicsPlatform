namespace MetaheuristicsPlatform.Execution;

/// <summary>Processes a half-open candidate range [startInclusive,endExclusive).</summary>
public delegate void CandidateRangeAction(
    int startInclusive,
    int endExclusive);