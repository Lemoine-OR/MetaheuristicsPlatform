using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.SA;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class SimulatedAnnealingTransitionBenchmarks
{
    private double[] _deltaSolution = null!;
    private double[] _fullSolution = null!;
    private IRandomSource _deltaRandom = null!;
    private IRandomSource _fullRandom = null!;

    private ReversibleTrajectoryStepExecutor<
        double[],
        SetComponentMove,
        double> _deltaExecutor = null!;

    private ReversibleTrajectoryStepExecutor<
        double[],
        SetComponentMove,
        double> _fullExecutor = null!;

    private SetComponentMove _move;

    [Params(128, 1024, 8192)]
    public int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _deltaSolution =
            new double[Dimension];

        _fullSolution =
            new double[Dimension];

        _move =
            new SetComponentMove(
                Index: Dimension / 2,
                NewValue: 1.0);

        var moveOperator =
            new SetComponentMoveOperator();

        _deltaExecutor =
            new ReversibleTrajectoryStepExecutor<
                double[],
                SetComponentMove,
                double>(
                moveOperator,
                EvaluateSphere,
                new MetropolisAcceptancePolicy(
                    temperature: 1e-9),
                new SphereSetDeltaEvaluator());

        _fullExecutor =
            new ReversibleTrajectoryStepExecutor<
                double[],
                SetComponentMove,
                double>(
                moveOperator,
                EvaluateSphere,
                new MetropolisAcceptancePolicy(
                    temperature: 1e-9));

        _deltaRandom =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(123UL);

        _fullRandom =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(456UL);
    }

    [Benchmark(Baseline = true)]
    public TrajectoryStepResult FullEvaluationReject()
    {
        SetComponentMove move =
            _move;

        return
            _fullExecutor.Execute(
                ref _fullSolution,
                currentObjective: 0.0,
                bestObjective: 0.0,
                in move,
                iteration: 1,
                OptimizationSense.Minimize,
                _fullRandom);
    }

    [Benchmark]
    public TrajectoryStepResult DeltaEvaluationReject()
    {
        SetComponentMove move =
            _move;

        return
            _deltaExecutor.Execute(
                ref _deltaSolution,
                currentObjective: 0.0,
                bestObjective: 0.0,
                in move,
                iteration: 1,
                OptimizationSense.Minimize,
                _deltaRandom);
    }

    private static double EvaluateSphere(
        in double[] solution)
    {
        double sum = 0.0;

        for (int i = 0;
             i < solution.Length;
             i++)
        {
            sum +=
                solution[i] *
                solution[i];
        }

        return sum;
    }

    private readonly record struct SetComponentMove(
        int Index,
        double NewValue);

    private sealed class SetComponentMoveOperator :
        IReversibleMoveOperator<
            double[],
            SetComponentMove,
            double>
    {
        public double CaptureUndo(
            in double[] solution,
            in SetComponentMove move) =>
            solution[move.Index];

        public void Apply(
            ref double[] solution,
            in SetComponentMove move)
        {
            solution[move.Index] =
                move.NewValue;
        }

        public void Undo(
            ref double[] solution,
            in SetComponentMove move,
            in double undo)
        {
            solution[move.Index] =
                undo;
        }
    }

    private sealed class SphereSetDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<
            double[],
            SetComponentMove>
    {
        public bool TryEvaluateCandidateObjective(
            in double[] solution,
            double currentObjective,
            in SetComponentMove move,
            out double candidateObjective)
        {
            double old =
                solution[move.Index];

            candidateObjective =
                currentObjective -
                old * old +
                move.NewValue *
                move.NewValue;

            return true;
        }
    }
}