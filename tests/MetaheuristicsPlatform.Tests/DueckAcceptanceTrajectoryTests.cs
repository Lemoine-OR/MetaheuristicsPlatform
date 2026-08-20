using MetaheuristicsPlatform.Algorithms.Acceptance;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Acceptance;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class DueckAcceptanceTrajectoryTests
{
    [Fact]
    public void GreatDelugeMinimizationUsesAbsoluteWaterLevel()
    {
        var p = new GreatDelugeAcceptancePolicy(10.0);
        var r = new FixedRandomSource();
        var a = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,1,8.0,10.0,7.0);
        var b = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,2,8.0,10.0001,7.0);

        Assert.True(p.ShouldAccept(in a,r));
        Assert.False(p.ShouldAccept(in b,r));
        Assert.Equal(0,r.NextDoubleCalls);
    }

    [Fact]
    public void GreatDelugeMaximizationMirrorsWaterLevel()
    {
        var p = new GreatDelugeAcceptancePolicy(10.0);
        var a = new TrajectoryAcceptanceContext(OptimizationSense.Maximize,1,12.0,10.0,14.0);
        var b = new TrajectoryAcceptanceContext(OptimizationSense.Maximize,2,12.0,9.9999,14.0);

        Assert.True(p.ShouldAccept(in a,new FixedRandomSource()));
        Assert.False(p.ShouldAccept(in b,new FixedRandomSource()));
    }

    [Fact]
    public void ClassicalGreatDelugeCanRejectCurrentImprovementAboveAdvancedLevel()
    {
        var p = new GreatDelugeAcceptancePolicy(5.0);
        var c = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,1,10.0,7.0,6.0);
        Assert.False(p.ShouldAccept(in c,new FixedRandomSource()));
    }

    [Fact]
    public void GreatDelugeLevelAdvancesLinearlyInBothSenses()
    {
        var min = new GreatDelugeAcceptancePolicy(10.0);
        min.AdvanceLevel(OptimizationSense.Minimize,0.25);
        Assert.Equal(9.75,min.WaterLevel,12);

        var max = new GreatDelugeAcceptancePolicy(10.0);
        max.AdvanceLevel(OptimizationSense.Maximize,0.25);
        Assert.Equal(10.25,max.WaterLevel,12);
    }

    [Fact]
    public void RecordToRecordTravelUsesBestRecordNotCurrentSolution()
    {
        var p = new RecordToRecordTravelAcceptancePolicy(2.0);
        var a = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,1,20.0,12.0,10.0);
        var b = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,2,20.0,12.0001,10.0);

        Assert.True(p.ShouldAccept(in a,new FixedRandomSource()));
        Assert.False(p.ShouldAccept(in b,new FixedRandomSource()));
    }

    [Fact]
    public void RecordToRecordTravelMirrorsMaximizationSense()
    {
        var p = new RecordToRecordTravelAcceptancePolicy(2.0);
        var a = new TrajectoryAcceptanceContext(OptimizationSense.Maximize,1,5.0,8.0,10.0);
        var b = new TrajectoryAcceptanceContext(OptimizationSense.Maximize,2,5.0,7.9999,10.0);

        Assert.True(p.ShouldAccept(in a,new FixedRandomSource()));
        Assert.False(p.ShouldAccept(in b,new FixedRandomSource()));
    }

    [Fact]
    public void ZeroRecordDeviationAcceptsOnlyRecordOrBetter()
    {
        var p = new RecordToRecordTravelAcceptancePolicy(0.0);
        var a = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,1,10.0,8.0,8.0);
        var b = new TrajectoryAcceptanceContext(OptimizationSense.Minimize,2,8.0,8.000001,8.0);

        Assert.True(p.ShouldAccept(in a,new FixedRandomSource()));
        Assert.False(p.ShouldAccept(in b,new FixedRandomSource()));
    }

    [Fact]
    public void GreatDelugeExactDeltaRejectsWithoutApplyingMove()
    {
        var op = new CountingIntMoveOperator();
        var a = new GreatDelugeOptimizer<int,int,int>(
            new ConstantInitial(0),new ConstantMove(+10),op,new IntDelta());

        var result = a.Optimize(
            new MinProblem(),
            new GreatDelugeParameters { RainSpeed = 0.1 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0,result.BestSolution);
        Assert.Equal(0,op.ApplyCalls);
        Assert.Equal(0,op.UndoCalls);
    }

    [Fact]
    public void RecordToRecordTravelExactDeltaRejectsOutsideRecordBand()
    {
        var op = new CountingIntMoveOperator();
        var a = new RecordToRecordTravelOptimizer<int,int,int>(
            new ConstantInitial(0),new ConstantMove(+10),op,new IntDelta());

        var result = a.Optimize(
            new MinProblem(),
            new RecordToRecordTravelParameters { Deviation = 1.0 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0,result.BestSolution);
        Assert.Equal(0,op.ApplyCalls);
    }

    [Fact]
    public void RecordToRecordTravelAcceptsWorseningMoveInsideRecordBand()
    {
        var op = new CountingIntMoveOperator();
        var a = new RecordToRecordTravelOptimizer<int,int,int>(
            new ConstantInitial(0),new ConstantMove(+1),op,new IntDelta());

        var result = a.Optimize(
            new MinProblem(),
            new RecordToRecordTravelParameters { Deviation = 2.0 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0,result.BestSolution);
        Assert.Equal(1,op.ApplyCalls);
    }

    [Fact]
    public void GreatDelugeDoesNotPromoteRejectedImprovingProbe()
    {
        var a = new GreatDelugeOptimizer<int,int,int>(
            new ConstantInitial(10),
            new SequenceMoves(-1,-1),
            new CountingIntMoveOperator(),
            new IntDelta());

        var result = a.Optimize(
            new MinProblem(),
            new GreatDelugeParameters { RainSpeed = 100.0 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(3),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(9,result.BestSolution);
    }

    [Fact]
    public void StableIdsAndCatalogExposeDueckMethods()
    {
        Assert.Equal("great-deluge-dueck-1993",MetaheuristicAlgorithmIds.GreatDeluge);
        Assert.Equal("record-to-record-travel-dueck-1993",MetaheuristicAlgorithmIds.RecordToRecordTravel);

        var g = MetaheuristicCatalog.GetRequired(MetaheuristicAlgorithmIds.GreatDeluge);
        var r = MetaheuristicCatalog.GetRequired(MetaheuristicAlgorithmIds.RecordToRecordTravel);

        Assert.True(g.RequiresComposition);
        Assert.True(r.RequiresComposition);
        Assert.Equal("10.1006/jcph.1993.1010",g.Doi);
        Assert.Equal("10.1006/jcph.1993.1010",r.Doi);
    }

    [Fact]
    public void InvalidMethodParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GreatDelugeParameters { RainSpeed = 0.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RecordToRecordTravelParameters { Deviation = -1.0 }.Validate());
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
        public int Create(IOptimizationProblem<int> problem,IRandomSource random) => _value;
    }

    private sealed class ConstantMove : IStochasticNeighborhood<int,int>
    {
        private readonly int _move;
        public ConstantMove(int move) => _move = move;
        public bool TrySampleMove(in int solution,IRandomSource random,out int move)
        {
            move = _move;
            return true;
        }
    }

    private sealed class SequenceMoves : IStochasticNeighborhood<int,int>
    {
        private readonly int[] _moves;
        private int _index;
        public SequenceMoves(params int[] moves) => _moves = moves;
        public bool TrySampleMove(in int solution,IRandomSource random,out int move)
        {
            if (_index >= _moves.Length) { move = default; return false; }
            move = _moves[_index++];
            return true;
        }
    }

    private sealed class CountingIntMoveOperator : IReversibleMoveOperator<int,int,int>
    {
        public int ApplyCalls { get; private set; }
        public int UndoCalls { get; private set; }
        public int CaptureUndo(in int solution,in int move) => solution;
        public void Apply(ref int solution,in int move) { ApplyCalls++; solution += move; }
        public void Undo(ref int solution,in int move,in int undo) { UndoCalls++; solution = undo; }
    }

    private sealed class IntDelta : IMoveObjectiveDeltaEvaluator<int,int>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,double currentObjective,in int move,out double candidateObjective)
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
        public double NextDouble() { NextDoubleCalls++; return 0.0; }
        public int NextInt32(int exclusiveMax) => 0;
        public int NextInt32(int inclusiveMin,int exclusiveMax) => inclusiveMin;
        public void Fill(Span<byte> buffer) => buffer.Clear();
    }
}