using MetaheuristicsPlatform.Algorithms.Acceptance;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class DemonBasedAcceptanceTests
{
    [Fact]
    public void MinimizationAcceptsWorseningCandidateWithinCredit()
    {
        var policy = new DemonAcceptancePolicy(3.0);
        var random = new FixedRandomSource();
        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 12.5, 9.0);

        Assert.True(policy.ShouldAccept(in context, random));
        Assert.Equal(0, random.NextDoubleCalls);
    }

    [Fact]
    public void MinimizationRejectsWorseningCandidateBeyondCredit()
    {
        var policy = new DemonAcceptancePolicy(2.0);
        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 12.5, 9.0);

        Assert.False(policy.ShouldAccept(in context, new FixedRandomSource()));
    }

    [Fact]
    public void AcceptedImprovementReplenishesCredit()
    {
        var policy = new DemonAcceptancePolicy(2.0);
        var step = new TrajectoryStepResult(
            true, true, true, false,
            10.0, 7.0, 7.0,
            TrajectoryTransitionQuality.Improving);

        policy.CompleteTransition(OptimizationSense.Minimize, in step);

        Assert.Equal(5.0, policy.Credit);
    }

    [Fact]
    public void AcceptedWorseningMoveSpendsCredit()
    {
        var policy = new DemonAcceptancePolicy(5.0);
        var step = new TrajectoryStepResult(
            true, true, true, false,
            10.0, 13.0, 13.0,
            TrajectoryTransitionQuality.Worsening);

        policy.CompleteTransition(OptimizationSense.Minimize, in step);

        Assert.Equal(2.0, policy.Credit);
    }

    [Fact]
    public void RejectedCandidateLeavesCreditUnchanged()
    {
        var policy = new DemonAcceptancePolicy(2.0);
        var step = new TrajectoryStepResult(
            false, true, false, false,
            10.0, 13.0, 10.0,
            TrajectoryTransitionQuality.Worsening);

        policy.CompleteTransition(OptimizationSense.Minimize, in step);

        Assert.Equal(2.0, policy.Credit);
    }

    [Fact]
    public void AcceptedMovesPreserveMinimizationEnergyInvariant()
    {
        var policy = new DemonAcceptancePolicy(4.0);
        const double initialTotal = 10.0 + 4.0;

        var improve = new TrajectoryStepResult(
            true, true, true, false,
            10.0, 8.0, 8.0,
            TrajectoryTransitionQuality.Improving);
        policy.CompleteTransition(OptimizationSense.Minimize, in improve);
        Assert.Equal(initialTotal, 8.0 + policy.Credit, 12);

        var worsen = new TrajectoryStepResult(
            true, true, true, false,
            8.0, 11.0, 11.0,
            TrajectoryTransitionQuality.Worsening);
        policy.CompleteTransition(OptimizationSense.Minimize, in worsen);
        Assert.Equal(initialTotal, 11.0 + policy.Credit, 12);
    }

    [Fact]
    public void MaximizationMirrorsDemonEnergyOrientation()
    {
        var policy = new DemonAcceptancePolicy(3.0);
        var accept = new TrajectoryAcceptanceContext(
            OptimizationSense.Maximize, 1, 10.0, 8.0, 12.0);
        var reject = new TrajectoryAcceptanceContext(
            OptimizationSense.Maximize, 1, 10.0, 6.0, 12.0);

        Assert.True(policy.ShouldAccept(in accept, new FixedRandomSource()));
        Assert.False(policy.ShouldAccept(in reject, new FixedRandomSource()));

        var step = new TrajectoryStepResult(
            true, true, true, false,
            10.0, 8.0, 8.0,
            TrajectoryTransitionQuality.Worsening);
        policy.CompleteTransition(OptimizationSense.Maximize, in step);
        Assert.Equal(1.0, policy.Credit);
    }

    [Fact]
    public void AcceptedMovesPreserveMaximizationEnergyInvariant()
    {
        var policy = new DemonAcceptancePolicy(4.0);
        const double initialTotal = -10.0 + 4.0;

        var improve = new TrajectoryStepResult(
            true, true, true, false,
            10.0, 12.0, 12.0,
            TrajectoryTransitionQuality.Improving);
        policy.CompleteTransition(OptimizationSense.Maximize, in improve);
        Assert.Equal(initialTotal, -12.0 + policy.Credit, 12);

        var worsen = new TrajectoryStepResult(
            true, true, true, false,
            12.0, 9.0, 9.0,
            TrajectoryTransitionQuality.Worsening);
        policy.CompleteTransition(OptimizationSense.Maximize, in worsen);
        Assert.Equal(initialTotal, -9.0 + policy.Credit, 12);
    }

    [Fact]
    public void ZeroInitialCreditStillAcceptsImprovementsAndAccumulatesBudget()
    {
        var policy = new DemonAcceptancePolicy(0.0);
        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 7.0, 7.0);

        Assert.True(policy.ShouldAccept(in context, new FixedRandomSource()));

        var step = new TrajectoryStepResult(
            true, true, true, false,
            10.0, 7.0, 7.0,
            TrajectoryTransitionQuality.Improving);
        policy.CompleteTransition(OptimizationSense.Minimize, in step);
        Assert.Equal(3.0, policy.Credit);
    }

    [Fact]
    public void ExactDeltaRejectionDoesNotApplyMove()
    {
        var op = new CountingIntMoveOperator();
        var algorithm = new DemonBasedAcceptanceOptimizer<int,int,int>(
            new ConstantInitial(0),
            new ConstantMove(+10),
            op,
            new IntDelta());

        var result = algorithm.Optimize(
            new MinProblem(),
            new DemonAcceptanceParameters { InitialCredit = 1.0 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, result.BestSolution);
        Assert.Equal(0, op.ApplyCalls);
        Assert.Equal(0, op.UndoCalls);
    }

    [Fact]
    public void StableIdAndCatalogExposeDemonBasedAcceptance()
    {
        Assert.Equal(
            "demon-based-acceptance-talbi-2009",
            MetaheuristicAlgorithmIds.DemonBasedAcceptance);

        var entry = MetaheuristicCatalog.GetRequired(
            MetaheuristicAlgorithmIds.DemonBasedAcceptance);

        Assert.True(entry.RequiresComposition);
        Assert.Equal("10.1002/9780470496916.ch2", entry.Doi);
    }

    [Fact]
    public void InvalidCreditIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DemonAcceptanceParameters { InitialCredit = -1.0 }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DemonAcceptanceParameters { InitialCredit = double.NaN }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DemonAcceptancePolicy(-0.1));
    }

    private sealed class MinProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) => solution;
    }

    private sealed class ConstantInitial : IAcceptanceTrajectoryInitialSolutionGenerator<int>
    {
        private readonly int _value;
        public ConstantInitial(int value) => _value = value;
        public int Create(IOptimizationProblem<int> problem, IRandomSource random) => _value;
    }

    private sealed class ConstantMove : IStochasticNeighborhood<int,int>
    {
        private readonly int _move;
        public ConstantMove(int move) => _move = move;

        public bool TrySampleMove(in int solution, IRandomSource random, out int move)
        {
            move = _move;
            return true;
        }
    }

    private sealed class CountingIntMoveOperator : IReversibleMoveOperator<int,int,int>
    {
        public int ApplyCalls { get; private set; }
        public int UndoCalls { get; private set; }

        public int CaptureUndo(in int solution, in int move) => solution;

        public void Apply(ref int solution, in int move)
        {
            ApplyCalls++;
            solution += move;
        }

        public void Undo(ref int solution, in int move, in int undo)
        {
            UndoCalls++;
            solution = undo;
        }
    }

    private sealed class IntDelta : IMoveObjectiveDeltaEvaluator<int,int>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in int move,
            out double candidateObjective)
        {
            candidateObjective = solution + move;
            return true;
        }
    }

    private sealed class FixedRandomSource : IRandomSource
    {
        public ulong Seed => 1UL;
        public int NextDoubleCalls { get; private set; }

        public ulong NextUInt64() => 0UL;
        public double NextDouble()
        {
            NextDoubleCalls++;
            return 0.0;
        }

        public int NextInt32(int exclusiveMax) => 0;
        public int NextInt32(int inclusiveMin, int exclusiveMax) => inclusiveMin;
        public void Fill(Span<byte> buffer) => buffer.Clear();
    }
}
