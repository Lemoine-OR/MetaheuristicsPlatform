namespace MetaheuristicsPlatform.Trajectory;

/// <summary>
/// Allocation-free trajectory counters.
/// </summary>
public struct TrajectoryStatisticsAccumulator
{
    public long Attempts { get; private set; }

    public long Accepted { get; private set; }

    public long Rejected =>
        Attempts - Accepted;

    public long Improving { get; private set; }

    public long Equal { get; private set; }

    public long Worsening { get; private set; }

    public long DeltaEvaluations { get; private set; }

    public long FullEvaluations { get; private set; }

    public long AppliedMoves { get; private set; }

    public long UndoneMoves { get; private set; }

    public double AcceptanceRatio =>
        Attempts == 0
            ? 0.0
            : (double)Accepted /
              Attempts;

    public void Record(
        in TrajectoryStepResult result)
    {
        Attempts++;

        if (result.Accepted)
        {
            Accepted++;
        }

        switch (result.Quality)
        {
            case TrajectoryTransitionQuality.Improving:
                Improving++;
                break;

            case TrajectoryTransitionQuality.Equal:
                Equal++;
                break;

            case TrajectoryTransitionQuality.Worsening:
                Worsening++;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (result.UsedDeltaEvaluation)
        {
            DeltaEvaluations++;
        }
        else
        {
            FullEvaluations++;
        }

        if (result.MoveApplied)
        {
            AppliedMoves++;
        }

        if (result.MoveUndone)
        {
            UndoneMoves++;
        }
    }
}