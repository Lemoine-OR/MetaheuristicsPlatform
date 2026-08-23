using MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedAdaptiveLargeNeighborhoodSearchTests
{
    [Fact]
    public void PairCoupledSegmentedRouletteLearnsJointPairWeight()
    {
        var parameters =
            new AdaptiveLargeNeighborhoodSearchParameters
            {
                SegmentLength = 1,
                ReactionFactor = 1.0,
                InitialOperatorWeight = 1.0
            };

        IAdaptiveLargeNeighborhoodOperatorSelectionSession session =
            new PairCoupledSegmentedRouletteOperatorSelectionStrategy()
                .CreateSession(
                    2,
                    2,
                    parameters);

        var firstRandom =
            new SequenceRandomSource(
                doubles: new[] { 0.0 });

        AdaptiveLargeNeighborhoodOperatorSelection first =
            session.Select(
                firstRandom,
                1);

        Assert.Equal(0, first.DestroyIndex);
        Assert.Equal(0, first.RepairIndex);

        session.RecordOutcome(
            in first,
            20.0);

        session.CompleteIteration(1);

        var secondRandom =
            new SequenceRandomSource(
                doubles: new[] { 0.5 });

        AdaptiveLargeNeighborhoodOperatorSelection second =
            session.Select(
                secondRandom,
                2);

        Assert.Equal(0, second.DestroyIndex);
        Assert.Equal(0, second.RepairIndex);
        Assert.Equal(1, session.SegmentUpdateCount);
    }

    [Fact]
    public void AlphaUcbExploresAllPairsBeforeExploitation()
    {
        var strategy =
            new AlphaUcbOperatorPairSelectionStrategy(
                alpha: 0.05,
                initialAverageReward: 1.0);

        IAdaptiveLargeNeighborhoodOperatorSelectionSession session =
            strategy.CreateSession(
                2,
                2,
                new AdaptiveLargeNeighborhoodSearchParameters());

        var random =
            new SequenceRandomSource(
                ints: new[] { 0, 0, 0, 0 });

        var seen =
            new HashSet<(int Destroy,int Repair)>();

        for (int iteration = 1; iteration <= 4; iteration++)
        {
            AdaptiveLargeNeighborhoodOperatorSelection selection =
                session.Select(
                    random,
                    iteration);

            seen.Add(
                (selection.DestroyIndex, selection.RepairIndex));

            double reward =
                selection.DestroyIndex == 0 &&
                selection.RepairIndex == 0
                    ? 20.0
                    : 0.0;

            session.RecordOutcome(
                in selection,
                reward);

            session.CompleteIteration(
                iteration);
        }

        Assert.Equal(4, seen.Count);

        AdaptiveLargeNeighborhoodOperatorSelection exploit =
            session.Select(
                random,
                5);

        Assert.Equal(0, exploit.DestroyIndex);
        Assert.Equal(0, exploit.RepairIndex);
        Assert.Equal(0, session.SegmentUpdateCount);
    }

    [Fact]
    public void ThresholdAdapterUsesTrajectoryAcceptanceWithoutRandomDraw()
    {
        ILargeNeighborhoodAcceptancePolicy acceptance =
            AdvancedAdaptiveLargeNeighborhoodAcceptance.Threshold(
                2.0);

        var random =
            new SequenceRandomSource();

        var accepted =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                11.5,
                9.0);

        var rejected =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                2,
                10.0,
                13.0,
                9.0);

        Assert.True(
            acceptance.ShouldAccept(
                in accepted,
                random));

        Assert.False(
            acceptance.ShouldAccept(
                in rejected,
                random));

        Assert.Equal(0, random.NextDoubleCalls);
    }

    [Fact]
    public void RecordToRecordAdapterUsesBestRecordDeviation()
    {
        ILargeNeighborhoodAcceptancePolicy acceptance =
            AdvancedAdaptiveLargeNeighborhoodAcceptance.RecordToRecordTravel(
                2.0);

        var random =
            new SequenceRandomSource();

        var accepted =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                11.0,
                9.0);

        var rejected =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                2,
                10.0,
                12.0,
                9.0);

        Assert.True(
            acceptance.ShouldAccept(
                in accepted,
                random));

        Assert.False(
            acceptance.ShouldAccept(
                in rejected,
                random));
    }

    [Fact]
    public void AlphaUcbParameterGuardsAreStrict()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AlphaUcbOperatorPairSelectionStrategy(alpha: -0.01));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AlphaUcbOperatorPairSelectionStrategy(alpha: 1.01));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AlphaUcbOperatorPairSelectionStrategy(
                alpha: 0.05,
                initialAverageReward: -1.0));
    }

    private sealed class SequenceRandomSource :
        IRandomSource
    {
        private readonly Queue<double> _doubles;
        private readonly Queue<int> _ints;

        public SequenceRandomSource(
            IEnumerable<double>? doubles = null,
            IEnumerable<int>? ints = null)
        {
            _doubles =
                new Queue<double>(
                    doubles ?? Array.Empty<double>());

            _ints =
                new Queue<int>(
                    ints ?? Array.Empty<int>());
        }

        public ulong Seed =>
            0UL;

        public int NextDoubleCalls { get; private set; }

        public ulong NextUInt64() =>
            0UL;

        public double NextDouble()
        {
            NextDoubleCalls++;

            return
                _doubles.Count > 0
                    ? _doubles.Dequeue()
                    : 0.0;
        }

        public int NextInt32(
            int exclusiveMax)
        {
            if (exclusiveMax <= 0)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            int value =
                _ints.Count > 0
                    ? _ints.Dequeue()
                    : 0;

            return
                Math.Abs(value) %
                exclusiveMax;
        }

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax)
        {
            if (inclusiveMin >= exclusiveMax)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            return
                inclusiveMin +
                NextInt32(
                    exclusiveMax - inclusiveMin);
        }

        public void Fill(
            Span<byte> buffer)
        {
            buffer.Clear();
        }
    }
}
