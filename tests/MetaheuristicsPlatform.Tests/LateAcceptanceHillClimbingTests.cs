using MetaheuristicsPlatform.Algorithms.Acceptance;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class LateAcceptanceHillClimbingTests
{
    [Fact]
    public void MinimizationAcceptsWorseningCandidateThatImprovesHistoryReference()
    {
        var policy = new LateAcceptancePolicy(2, 10.0);
        var random = new FixedRandomSource();
        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 8.0, 9.0, 7.0);

        Assert.True(policy.ShouldAccept(in context, random));
        Assert.Equal(0, random.NextDoubleCalls);
    }

    [Fact]
    public void FinalFormAlsoAcceptsCandidateThatIsNotWorseThanCurrent()
    {
        var policy = new LateAcceptancePolicy(1, 5.0);
        policy.CompleteTransition(OptimizationSense.Minimize, 5.0);

        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 2, 10.0, 9.0, 5.0);

        Assert.True(policy.ShouldAccept(in context, new FixedRandomSource()));
    }

    [Fact]
    public void CandidateWorseThanCurrentAndHistoryIsRejected()
    {
        var policy = new LateAcceptancePolicy(3, 10.0);
        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 9.0, 11.0, 8.0);

        Assert.False(policy.ShouldAccept(in context, new FixedRandomSource()));
    }

    [Fact]
    public void HistoryEntriesImproveMonotonically()
    {
        var policy = new LateAcceptancePolicy(1, 10.0);

        policy.CompleteTransition(OptimizationSense.Minimize, 12.0);
        Assert.Equal(10.0, policy.CurrentReference);

        policy.CompleteTransition(OptimizationSense.Minimize, 8.0);
        Assert.Equal(8.0, policy.CurrentReference);
    }

    [Fact]
    public void MaximizationMirrorsLateAcceptanceRule()
    {
        var policy = new LateAcceptancePolicy(2, 10.0);
        var context = new TrajectoryAcceptanceContext(
            OptimizationSense.Maximize, 1, 12.0, 11.0, 14.0);

        Assert.True(policy.ShouldAccept(in context, new FixedRandomSource()));

        var rejected = new TrajectoryAcceptanceContext(
            OptimizationSense.Maximize, 2, 12.0, 9.0, 14.0);

        Assert.False(policy.ShouldAccept(in rejected, new FixedRandomSource()));
    }

    [Fact]
    public void HistoryLengthOneReducesAcceptanceToHillClimbing()
    {
        var policy = new LateAcceptancePolicy(1, 10.0);

        var improving = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 9.0, 9.0);
        var worsening = new TrajectoryAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 10.1, 9.0);

        Assert.True(policy.ShouldAccept(in improving, new FixedRandomSource()));
        Assert.False(policy.ShouldAccept(in worsening, new FixedRandomSource()));
    }

    [Fact]
    public void ExactDeltaRejectionDoesNotApplyMove()
    {
        var op = new CountingIntMoveOperator();
        var algorithm = new LateAcceptanceHillClimbingOptimizer<int,int,int>(
            new ConstantInitial(0),
            new ConstantMove(+10),
            op,
            new IntDelta());

        var result = algorithm.Optimize(
            new MinProblem(),
            new LateAcceptanceParameters { HistoryLength = 4 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, result.BestSolution);
        Assert.Equal(0, op.ApplyCalls);
        Assert.Equal(0, op.UndoCalls);
    }

    [Fact]
    public void StableIdAndCatalogExposeLateAcceptance()
    {
        Assert.Equal(
            "late-acceptance-hill-climbing-burke-bykov-2017",
            MetaheuristicAlgorithmIds.LateAcceptanceHillClimbing);

        var entry = MetaheuristicCatalog.GetRequired(
            MetaheuristicAlgorithmIds.LateAcceptanceHillClimbing);

        Assert.True(entry.RequiresComposition);
        Assert.Equal("10.1016/j.ejor.2016.07.012", entry.Doi);
    }

    [Fact]
    public void InvalidHistoryLengthIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LateAcceptanceParameters { HistoryLength = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LateAcceptancePolicy(0, 1.0));
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
