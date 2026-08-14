using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class ReversibleTrajectoryStepExecutorTests
{
    [Fact]
    public void DeltaRejectedMoveIsNeverApplied()
    {
        var moveOperator =
            new CountingAddMoveOperator();

        var executor =
            new ReversibleTrajectoryStepExecutor<
                IntSolution,
                AddMove,
                int>(
                moveOperator,
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
            new(+5);

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

        Assert.True(
            result.UsedDeltaEvaluation);

        Assert.False(
            result.MoveApplied);

        Assert.False(
            result.MoveUndone);

        Assert.Equal(
            10,
            solution.Value);

        Assert.Equal(
            0,
            moveOperator.ApplyCount);

        Assert.Equal(
            0,
            moveOperator.UndoCount);
    }

    [Fact]
    public void DeltaAcceptedMoveIsAppliedOnceWithoutUndo()
    {
        var moveOperator =
            new CountingAddMoveOperator();

        var executor =
            new ReversibleTrajectoryStepExecutor<
                IntSolution,
                AddMove,
                int>(
                moveOperator,
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
            new(-3);

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
            7,
            solution.Value);

        Assert.Equal(
            1,
            moveOperator.ApplyCount);

        Assert.Equal(
            0,
            moveOperator.UndoCount);
    }

    [Fact]
    public void FullEvaluationRejectedMoveIsUndone()
    {
        var moveOperator =
            new CountingAddMoveOperator();

        var executor =
            new ReversibleTrajectoryStepExecutor<
                IntSolution,
                AddMove,
                int>(
                moveOperator,
                static (
                    in IntSolution solution) =>
                    solution.Value,
                new GreedyAcceptancePolicy());

        IntSolution solution =
            new(10);

        IRandomSource random =
            CreateRandom();

        AddMove move =
            new(+4);

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

        Assert.False(
            result.UsedDeltaEvaluation);

        Assert.True(
            result.MoveApplied);

        Assert.True(
            result.MoveUndone);

        Assert.Equal(
            10,
            solution.Value);

        Assert.Equal(
            1,
            moveOperator.ApplyCount);

        Assert.Equal(
            1,
            moveOperator.UndoCount);
    }

    [Fact]
    public void EvaluationExceptionRestoresSolution()
    {
        var moveOperator =
            new CountingAddMoveOperator();

        var executor =
            new ReversibleTrajectoryStepExecutor<
                IntSolution,
                AddMove,
                int>(
                moveOperator,
                static (
                    in IntSolution _) =>
                    throw new InvalidOperationException(
                        "test"),
                new GreedyAcceptancePolicy());

        IntSolution solution =
            new(10);

        IRandomSource random =
            CreateRandom();

        AddMove move =
            new(+4);

        Assert.Throws<InvalidOperationException>(
            () =>
                executor.Execute(
                    ref solution,
                    currentObjective: 10.0,
                    bestObjective: 10.0,
                    in move,
                    iteration: 1,
                    OptimizationSense.Minimize,
                    random,
                    TestContext.Current.CancellationToken));

        Assert.Equal(
            10,
            solution.Value);

        Assert.Equal(
            1,
            moveOperator.UndoCount);
    }

    private static IRandomSource CreateRandom() =>
        Xoshiro256StarStarRandomSourceFactory
            .Instance
            .Create(123UL);

    private readonly record struct IntSolution(
        int Value);

    private readonly record struct AddMove(
        int Delta);

    private sealed class CountingAddMoveOperator :
        IReversibleMoveOperator<
            IntSolution,
            AddMove,
            int>
    {
        public int ApplyCount { get; private set; }

        public int UndoCount { get; private set; }

        public int CaptureUndo(
            in IntSolution solution,
            in AddMove move) =>
            solution.Value;

        public void Apply(
            ref IntSolution solution,
            in AddMove move)
        {
            ApplyCount++;

            solution =
                new IntSolution(
                    solution.Value +
                    move.Delta);
        }

        public void Undo(
            ref IntSolution solution,
            in AddMove move,
            in int undo)
        {
            UndoCount++;

            solution =
                new IntSolution(
                    undo);
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