namespace MetaheuristicsPlatform.Trajectory;

public readonly record struct TrajectoryStepResult(
    bool Accepted,
    bool UsedDeltaEvaluation,
    bool MoveApplied,
    bool MoveUndone,
    double PreviousObjective,
    double CandidateObjective,
    double ResultingObjective,
    TrajectoryTransitionQuality Quality)
{
    public bool WasImproving =>
        Quality ==
        TrajectoryTransitionQuality.Improving;

    public bool WasEqual =>
        Quality ==
        TrajectoryTransitionQuality.Equal;

    public bool WasWorsening =>
        Quality ==
        TrajectoryTransitionQuality.Worsening;
}