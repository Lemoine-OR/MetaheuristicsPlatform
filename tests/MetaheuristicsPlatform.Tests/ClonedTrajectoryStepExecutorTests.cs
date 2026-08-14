using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class ClonedTrajectoryStepExecutorTests
{
    [Fact]
    public void DeltaRejectedMoveDoesNotClone()
    {
        int cloneCount = 0;

        var executor =
            new ClonedTrajectoryStepExecutor<
                IntSolution,
                AddMove>(
                new AddMoveOperator(),
                (
                    in IntSolution solution) =>
                {
                    cloneCount++;
                    return solution;
                },
                static (
                    in IntSolution solution) =>
                    solution.Value,
                new GreedyAcceptancePolicy(),
                new AddMoveDeltaEvaluator());

        IntSolution solution =
            new(10);

        IRandomSource random =
            CreateRandom();

        AddMove move =
            new(+1);

        TrajectoryStepResult result =
            executor.Execute(
                ref solution,
                currentObjective: 10.0,
                bestObjective: 10.0,
                in move,
                iteration: 1,
                OptimizationSense.Minimize,
                random,
                TestContext.Current.CancellationToken);

        Assert.False(
            result.Accepted);

        Assert.Equal(
            0,
            cloneCount);

        Assert.Equal(
            10,
            solution.Value);
    }

    [Fact]
    public void AcceptedMoveReplacesValueTypeSolution()
    {
        var executor =
            new ClonedTrajectoryStepExecutor<
                IntSolution,
                AddMove>(
                new AddMoveOperator(),
                static (
                    in IntSolution solution) =>
                    solution,
                static (
                    in IntSolution solution) =>
                    solution.Value,
                new GreedyAcceptancePolicy());

        IntSolution solution =
            new(10);

        IRandomSource random =
            CreateRandom();

        AddMove move =
            new(-2);

        TrajectoryStepResult result =
            executor.Execute(
                ref solution,
                currentObjective: 10.0,
                bestObjective: 10.0,
                in move,
                iteration: 1,
                OptimizationSense.Minimize,
                random,
                TestContext.Current.CancellationToken);

        Assert.True(
            result.Accepted);

        Assert.Equal(
            8,
            solution.Value);
    }

    private static IRandomSource CreateRandom() =>
        Xoshiro256StarStarRandomSourceFactory
            .Instance
            .Create(456UL);

    private readonly record struct IntSolution(
        int Value);

    private readonly record struct AddMove(
        int Delta);

    private sealed class AddMoveOperator :
        IMoveOperator<
            IntSolution,
            AddMove>
    {
        public void Apply(
            ref IntSolution solution,
            in AddMove move)
        {
            solution =
                new IntSolution(
                    solution.Value +
                    move.Delta);
        }
    }

    private sealed class AddMoveDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<
            IntSolution,
            AddMove>
    {
        public bool TryEvaluateCandidateObjective(
            in IntSolution solution,
            double currentObjective,
            in AddMove move,
            out double candidateObjective)
        {
            candidateObjective =
                currentObjective +
                move.Delta;

            return true;
        }
    }
}