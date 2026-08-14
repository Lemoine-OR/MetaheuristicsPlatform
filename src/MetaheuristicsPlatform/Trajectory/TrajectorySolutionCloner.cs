namespace MetaheuristicsPlatform.Trajectory;

/// <summary>
/// Clone/snapshot delegate used by the non-reversible fallback executor.
/// </summary>
public delegate TSolution TrajectorySolutionCloner<TSolution>(
    in TSolution solution);