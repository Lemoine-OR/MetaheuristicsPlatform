using MetaheuristicsPlatform.Algorithms.TA;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class ThresholdAcceptingTests
{
    [Fact]
    public void AcceptancePolicyAcceptsImprovingAndEqualMoves()
    {
        var policy =
            new ThresholdAcceptancePolicy(0.0);

        var random =
            new FixedRandomSource();

        var improving =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                9.0,
                10.0);

        var equal =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Minimize,
                2,
                10.0,
                10.0,
                9.0);

        Assert.True(
            policy.ShouldAccept(
                in improving,
                random));

        Assert.True(
            policy.ShouldAccept(
                in equal,
                random));
    }

    [Fact]
    public void AcceptancePolicyUsesDeterministicWorseningThreshold()
    {
        var policy =
            new ThresholdAcceptancePolicy(2.0);

        var random =
            new FixedRandomSource();

        var boundary =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                12.0,
                9.0);

        var outside =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Minimize,
                2,
                10.0,
                12.0001,
                9.0);

        Assert.True(
            policy.ShouldAccept(
                in boundary,
                random));

        Assert.False(
            policy.ShouldAccept(
                in outside,
                random));

        Assert.Equal(
            0,
            random.NextDoubleCalls);
    }

    [Fact]
    public void AcceptancePolicyMirrorsMaximizationSense()
    {
        var policy =
            new ThresholdAcceptancePolicy(1.5);

        var random =
            new FixedRandomSource();

        var accepted =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Maximize,
                1,
                10.0,
                8.5,
                12.0);

        var rejected =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Maximize,
                2,
                10.0,
                8.4,
                12.0);

        Assert.True(
            policy.ShouldAccept(
                in accepted,
                random));

        Assert.False(
            policy.ShouldAccept(
                in rejected,
                random));
    }

    [Fact]
    public void ZeroThresholdReducesAcceptanceToNonWorseningMoves()
    {
        var policy =
            new ThresholdAcceptancePolicy(0.0);

        var worsening =
            new TrajectoryAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                3.0,
                3.000001,
                2.0);

        Assert.False(
            policy.ShouldAccept(
                in worsening,
                new FixedRandomSource()));
    }

    [Fact]
    public void LinearScheduleReachesZeroExactly()
    {
        var schedule =
            new LinearThresholdSchedule(0.25);

        var context =
            new ThresholdAcceptingScheduleContext(
                1,
                100,
                50,
                1.0,
                0.2);

        Assert.Equal(
            0.0,
            schedule.GetNextThreshold(
                in context));
    }

    [Fact]
    public void GeometricScheduleContractsThreshold()
    {
        var schedule =
            new GeometricThresholdSchedule(0.8);

        var context =
            new ThresholdAcceptingScheduleContext(
                1,
                100,
                50,
                10.0,
                5.0);

        Assert.Equal(
            4.0,
            schedule.GetNextThreshold(
                in context),
            12);
    }

    [Fact]
    public void ExplicitScheduleRequiresMonotoneThresholdSequence()
    {
        Assert.Throws<ArgumentException>(() =>
            new ExplicitThresholdSchedule(
                new[]
                {
                    1.0,
                    1.1,
                    0.0
                }));

        var schedule =
            new ExplicitThresholdSchedule(
                new[]
                {
                    0.8,
                    0.3,
                    0.0
                });

        var first =
            new ThresholdAcceptingScheduleContext(
                1,
                0,
                0,
                1.0,
                1.0);

        var third =
            new ThresholdAcceptingScheduleContext(
                3,
                0,
                0,
                1.0,
                0.3);

        Assert.Equal(
            0.8,
            schedule.GetNextThreshold(
                in first));

        Assert.Equal(
            0.0,
            schedule.GetNextThreshold(
                in third));
    }

    [Fact]
    public void OptimizerDeltaFastPathDoesNotApplyRejectedMove()
    {
        var moveOperator =
            new CountingIntMoveOperator();

        var optimizer =
            new ThresholdAcceptingOptimizer<
                int,
                int,
                int>(
                new ConstantInitialSolutionGenerator(0),
                new ConstantMoveNeighborhood(+10),
                moveOperator,
                new IntDeltaEvaluator());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new IdentityMinimizationProblem(),
                new ThresholdAcceptingParameters
                {
                    InitialThreshold = 1.0,
                    MinimumThreshold = 0.0,
                    TransitionsPerThresholdLevel = 100,
                    StopAtMinimumThreshold = false
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(2),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);

        Assert.Equal(
            0,
            result.BestSolution);

        Assert.Equal(
            0,
            moveOperator.ApplyCalls);

        Assert.Equal(
            0,
            moveOperator.UndoCalls);
    }

    [Fact]
    public void OptimizerAdvancesThresholdLevelsAndStopsAtMinimum()
    {
        var optimizer =
            new ThresholdAcceptingOptimizer<
                int,
                int,
                int>(
                new ConstantInitialSolutionGenerator(5),
                new ConstantMoveNeighborhood(-1),
                new CountingIntMoveOperator(),
                new IntDeltaEvaluator());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new IdentityMinimizationProblem(),
                new ThresholdAcceptingParameters
                {
                    InitialThreshold = 1.0,
                    MinimumThreshold = 0.0,
                    TransitionsPerThresholdLevel = 1,
                    ThresholdSchedule =
                        ThresholdAcceptingScheduleKind.Linear,
                    LinearDecrement = 0.5,
                    StopAtMinimumThreshold = true
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            "MinimumThreshold",
            result.StopDecision.Criterion);

        Assert.Equal(
            3,
            result.BestSolution);
    }

    [Fact]
    public void StableIdCatalogAndDescriptorExposeThresholdAccepting()
    {
        Assert.Equal(
            "threshold-accepting-dueck-scheuer-1990",
            MetaheuristicAlgorithmIds.ThresholdAccepting);

        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.ThresholdAccepting);

        Assert.True(
            entry.RequiresComposition);

        var optimizer =
            new ThresholdAcceptingOptimizer<
                int,
                int,
                int>(
                new ConstantInitialSolutionGenerator(0),
                new ConstantMoveNeighborhood(1),
                new CountingIntMoveOperator(),
                new IntDeltaEvaluator());

        Assert.Equal(
            MetaheuristicAlgorithmIds.ThresholdAccepting,
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/0021-9991(90)90201-B");
    }

    [Fact]
    public void ParametersRejectInvalidThresholdControls()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ThresholdAcceptingParameters
            {
                InitialThreshold = -1.0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ThresholdAcceptingParameters
            {
                InitialThreshold = 1.0,
                MinimumThreshold = 2.0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ThresholdAcceptingParameters
            {
                TransitionsPerThresholdLevel = 0
            }.Validate());

        Assert.Throws<InvalidOperationException>(() =>
            new ThresholdAcceptingParameters
            {
                ThresholdSchedule =
                    ThresholdAcceptingScheduleKind.Explicit
            }.Validate());
    }

    private sealed class IdentityMinimizationProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution) =>
            solution;
    }

    private sealed class ConstantInitialSolutionGenerator :
        IThresholdAcceptingInitialSolutionGenerator<int>
    {
        private readonly int _value;

        public ConstantInitialSolutionGenerator(
            int value)
        {
            _value = value;
        }

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            _value;
    }

    private sealed class ConstantMoveNeighborhood :
        IStochasticNeighborhood<int, int>
    {
        private readonly int _move;

        public ConstantMoveNeighborhood(
            int move)
        {
            _move = move;
        }

        public bool TrySampleMove(
            in int solution,
            IRandomSource random,
            out int move)
        {
            move =
                _move;

            return true;
        }
    }

    private sealed class CountingIntMoveOperator :
        IReversibleMoveOperator<int, int, int>
    {
        public int ApplyCalls { get; private set; }

        public int UndoCalls { get; private set; }

        public int CaptureUndo(
            in int solution,
            in int move) =>
            solution;

        public void Apply(
            ref int solution,
            in int move)
        {
            ApplyCalls++;
            solution +=
                move;
        }

        public void Undo(
            ref int solution,
            in int move,
            in int undo)
        {
            UndoCalls++;
            solution =
                undo;
        }
    }

    private sealed class IntDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<int, int>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in int move,
            out double candidateObjective)
        {
            candidateObjective =
                solution +
                move;

            return true;
        }
    }

    private sealed class FixedRandomSource :
        IRandomSource
    {
        public ulong Seed => 1UL;

        public int NextDoubleCalls { get; private set; }

        public ulong NextUInt64() =>
            0UL;

        public double NextDouble()
        {
            NextDoubleCalls++;
            return 0.0;
        }

        public int NextInt32(
            int exclusiveMax) =>
            0;

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax) =>
            inclusiveMin;

        public void Fill(
            Span<byte> buffer) =>
            buffer.Clear();
    }
}