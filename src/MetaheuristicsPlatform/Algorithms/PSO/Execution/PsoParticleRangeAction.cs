namespace MetaheuristicsPlatform.Algorithms.PSO.Execution;

/// <summary>
/// Processes a contiguous half-open particle range [startInclusive, endExclusive).
/// </summary>
public delegate void PsoParticleRangeAction(
    int startInclusive,
    int endExclusive);