using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class AdvancedAdaptiveLargeNeighborhoodSearchBenchmarks
{
    private readonly AdaptiveLargeNeighborhoodSearchParameters _parameters =
        new()
        {
            SegmentLength = 100,
            ReactionFactor = 0.1,
            InitialOperatorWeight = 1.0
        };

    [Benchmark]
    public int AlphaUcbPairSelection()
    {
        IAdaptiveLargeNeighborhoodOperatorSelectionSession session =
            new AlphaUcbOperatorPairSelectionStrategy(
                alpha: 0.05)
                .CreateSession(
                    8,
                    6,
                    _parameters);

        var random =
            new Xoshiro256StarStarRandomSource(
                123456UL);

        int checksum =
            0;

        for (int iteration = 1; iteration <= 1000; iteration++)
        {
            AdaptiveLargeNeighborhoodOperatorSelection selection =
                session.Select(
                    random,
                    iteration);

            checksum +=
                selection.DestroyIndex +
                selection.RepairIndex;

            session.RecordOutcome(
                in selection,
                (iteration % 17) + 1.0);

            session.CompleteIteration(
                iteration);
        }

        return checksum;
    }

    [Benchmark]
    public int PairCoupledSegmentedRouletteSelection()
    {
        IAdaptiveLargeNeighborhoodOperatorSelectionSession session =
            new PairCoupledSegmentedRouletteOperatorSelectionStrategy()
                .CreateSession(
                    8,
                    6,
                    _parameters);

        var random =
            new Xoshiro256StarStarRandomSource(
                123456UL);

        int checksum =
            0;

        for (int iteration = 1; iteration <= 1000; iteration++)
        {
            AdaptiveLargeNeighborhoodOperatorSelection selection =
                session.Select(
                    random,
                    iteration);

            checksum +=
                selection.DestroyIndex +
                selection.RepairIndex;

            session.RecordOutcome(
                in selection,
                (iteration % 17) + 1.0);

            session.CompleteIteration(
                iteration);
        }

        return checksum;
    }
}
