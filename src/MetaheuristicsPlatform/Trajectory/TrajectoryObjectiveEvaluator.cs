namespace MetaheuristicsPlatform.Trajectory;

/// <summary>
/// Allocation-free-friendly objective delegate for trajectory algorithms.
/// </summary>
public delegate double TrajectoryObjectiveEvaluator<TSolution>(
    in TSolution solution);