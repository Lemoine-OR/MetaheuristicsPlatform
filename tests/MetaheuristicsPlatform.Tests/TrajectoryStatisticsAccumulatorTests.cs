using MetaheuristicsPlatform.Trajectory;

namespace MetaheuristicsPlatform.Tests;

public sealed class TrajectoryStatisticsAccumulatorTests
{
    [Fact]
    public void StatisticsSeparateEvaluationAndTransitionKinds()
    {
        var statistics =
            new TrajectoryStatisticsAccumulator();

        var improving =
            new TrajectoryStepResult(
                Accepted: true,
                UsedDeltaEvaluation: true,
                MoveApplied: true,
                MoveUndone: false,
                PreviousObjective: 10.0,
                CandidateObjective: 8.0,
                ResultingObjective: 8.0,
                Quality:
                    TrajectoryTransitionQuality.Improving);

        var worsening =
            new TrajectoryStepResult(
                Accepted: false,
                UsedDeltaEvaluation: false,
                MoveApplied: true,
                MoveUndone: true,
                PreviousObjective: 8.0,
                CandidateObjective: 9.0,
                ResultingObjective: 8.0,
                Quality:
                    TrajectoryTransitionQuality.Worsening);

        statistics.Record(
            in improving);

        statistics.Record(
            in worsening);

        Assert.Equal(
            2,
            statistics.Attempts);

        Assert.Equal(
            1,
            statistics.Accepted);

        Assert.Equal(
            1,
            statistics.Rejected);

        Assert.Equal(
            1,
            statistics.Improving);

        Assert.Equal(
            1,
            statistics.Worsening);

        Assert.Equal(
            1,
            statistics.DeltaEvaluations);

        Assert.Equal(
            1,
            statistics.FullEvaluations);

        Assert.Equal(
            2,
            statistics.AppliedMoves);

        Assert.Equal(
            1,
            statistics.UndoneMoves);

        Assert.Equal(
            0.5,
            statistics.AcceptanceRatio);
    }
}