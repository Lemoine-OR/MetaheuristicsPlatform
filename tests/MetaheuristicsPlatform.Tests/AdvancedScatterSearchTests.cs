using MetaheuristicsPlatform.Algorithms.ScatterSearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedScatterSearchTests
{
    [Fact]
    public void AdvancedComponentIdsAreStable()
    {
        Assert.Equal(
            "ss.refset.update.dynamic-refresh",
            ScatterSearchComponentIds.DynamicRefSetRefresh);

        Assert.Equal(
            "ss.refset.update.two-tier",
            ScatterSearchComponentIds.TwoTierRefSetUpdate);

        Assert.Equal(
            "ss.refset.rebuild.max-min",
            ScatterSearchComponentIds.MaxMinRefSetRebuild);

        Assert.Equal(
            "ss.diversity.minimum-distance",
            ScatterSearchComponentIds.MinimumDiversity);

        Assert.Equal(
            "ss.subsets.glover-types-1-4",
            ScatterSearchComponentIds.GloverSubsetTypesOneToFour);
    }

    [Fact]
    public void TwoTierUpdateImprovesQualityTier()
    {
        var updater =
            new TwoTierScatterSearchReferenceSetUpdateMethod<int>(
                qualityTierSize: 2);

        var referenceSet =
            BuildReferenceSet(
                updater,
                0, 10, 100, 200);

        bool accepted =
            updater.TryUpdate(
                referenceSet,
                Point(5),
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.True(accepted);
        Assert.Contains(referenceSet.Take(2), point => point.Solution == 0);
        Assert.Contains(referenceSet.Take(2), point => point.Solution == 5);
    }

    [Fact]
    public void TwoTierUpdateCanImproveDiversityTierWithoutImprovingQuality()
    {
        var updater =
            new TwoTierScatterSearchReferenceSetUpdateMethod<int>(
                qualityTierSize: 2);

        var referenceSet =
            BuildReferenceSet(
                updater,
                0, 10, 100, 200);

        bool accepted =
            updater.TryUpdate(
                referenceSet,
                Point(500),
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.True(accepted);
        Assert.Contains(
            referenceSet.Skip(2),
            point => point.Solution == 500);
    }

    [Fact]
    public void MinimumDiversityThresholdFiltersQualityTier()
    {
        var updater =
            new TwoTierScatterSearchReferenceSetUpdateMethod<int>(
                qualityTierSize: 2,
                minimumQualityDistance: 5.0);

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>();

        updater.Initialize(
            referenceSet,
            new[]
            {
                Point(0),
                Point(1),
                Point(10),
                Point(20)
            },
            referenceSetSize: 3,
            qualityReferenceSetSize: 2,
            new AbsoluteDistance(),
            OptimizationSense.Minimize,
            new ImmutableSolutionCloner<int>());

        Assert.Equal(0, referenceSet[0].Solution);
        Assert.Equal(10, referenceSet[1].Solution);
    }

    [Fact]
    public void GloverSubsetFamiliesGenerateRepresentativeTypesOneToFour()
    {
        var generator =
            new GloverScatterSearchSubsetGenerationMethod<int>(
                OptimizationSense.Minimize);

        var referenceSet =
            Enumerable
                .Range(0, 6)
                .Select(Point)
                .ToArray();

        IReadOnlyList<ScatterSearchSubset<int>> subsets =
            generator.Generate(referenceSet);

        Assert.Equal(33, subsets.Count);
        Assert.Contains(subsets, subset => subset.Members.Count == 2);
        Assert.Contains(subsets, subset => subset.Members.Count == 3);
        Assert.Contains(subsets, subset => subset.Members.Count == 4);
        Assert.Contains(subsets, subset => subset.Members.Count == 5);
        Assert.Contains(subsets, subset => subset.Members.Count == 6);
    }

    [Fact]
    public void MaxMinRebuildPreservesQualityTierAndRefreshesDiversityTier()
    {
        var rebuilder =
            new MaxMinScatterSearchReferenceSetRebuildingMethod<int>();

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>
            {
                Point(0),
                Point(1),
                Point(10),
                Point(20)
            };

        bool rebuilt =
            rebuilder.TryRebuild(
                referenceSet,
                new[]
                {
                    Point(50),
                    Point(100),
                    Point(200)
                },
                qualityReferenceSetSize: 2,
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.True(rebuilt);
        Assert.Equal(0, referenceSet[0].Solution);
        Assert.Equal(1, referenceSet[1].Solution);
        Assert.Contains(referenceSet.Skip(2), point => point.Solution == 200);
        Assert.All(referenceSet.Skip(2), point => Assert.True(point.IsNew));
        Assert.All(referenceSet.Take(2), point => Assert.False(point.IsNew));
    }

    [Fact]
    public void DynamicRefreshCombinesNewReferenceSolutionBeforeStaleScheduleContinues()
    {
        var combinationCalls =
            new List<int[]>();

        int combinationCall = 0;

        var combination =
            new DelegateScatterSearchSolutionCombinationMethod<int>(
                (subset, problem, random) =>
                {
                    combinationCalls.Add(
                        subset.Members
                            .Select(static point => point.Solution)
                            .ToArray());

                    combinationCall++;

                    return combinationCall == 1
                        ? new[] { -1 }
                        : new[] { 1000 };
                });

        var optimizer =
            new ScatterSearchOptimizer<int>(
                new QueueDiversification(0, 10, 20, 30),
                improvement: null,
                new ClassicalScatterSearchReferenceSetUpdateMethod<int>(),
                new PairwiseNewScatterSearchSubsetGenerationMethod<int>(),
                combination,
                new AbsoluteDistance(),
                referenceSetRebuilding: null);

        _ = optimizer.Optimize(
            new MinProblem(),
            new ScatterSearchParameters
            {
                DiversificationPopulationSize = 4,
                ReferenceSetSize = 3,
                QualityReferenceSetSize = 2,
                MaximumIterations = 5,
                ReferenceSetRefreshMode =
                    ScatterSearchReferenceSetRefreshMode.DynamicImmediate
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(combinationCalls.Count >= 2);
        Assert.Contains(-1, combinationCalls[1]);
    }

    [Fact]
    public void DiversityReplacementExcludesTheMemberBeingReplaced()
    {
        var updater =
            new TwoTierScatterSearchReferenceSetUpdateMethod<int>(
                qualityTierSize: 2);

        var referenceSet =
            BuildReferenceSet(
                updater,
                0, 10, 100, 200);

        bool accepted =
            updater.TryUpdate(
                referenceSet,
                Point(105),
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.True(accepted);
        Assert.DoesNotContain(
            referenceSet.Skip(2),
            point => point.Solution == 100);
        Assert.Contains(
            referenceSet.Skip(2),
            point => point.Solution == 105);
    }

    [Fact]
    public void RebuildPreservesBestQualityMembersRegardlessOfStorageOrder()
    {
        var rebuilder =
            new MaxMinScatterSearchReferenceSetRebuildingMethod<int>();

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>
            {
                Point(50),
                Point(0),
                Point(100),
                Point(10)
            };

        bool rebuilt =
            rebuilder.TryRebuild(
                referenceSet,
                new[]
                {
                    Point(200),
                    Point(300),
                    Point(400)
                },
                qualityReferenceSetSize: 2,
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.True(rebuilt);
        Assert.Equal(0, referenceSet[0].Solution);
        Assert.Equal(10, referenceSet[1].Solution);
        Assert.All(
            referenceSet.Take(2),
            point => Assert.False(point.IsNew));
    }
    private static List<ScatterSearchReferencePoint<int>> BuildReferenceSet(
        IScatterSearchReferenceSetUpdateMethod<int> updater,
        params int[] values)
    {
        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>();

        updater.Initialize(
            referenceSet,
            values.Select(Point).ToArray(),
            referenceSetSize: values.Length,
            qualityReferenceSetSize: 2,
            new AbsoluteDistance(),
            OptimizationSense.Minimize,
            new ImmutableSolutionCloner<int>());

        return referenceSet;
    }

    private static ScatterSearchReferencePoint<int> Point(
        int value) =>
        new(
            value,
            value,
            isNew: true);

    private sealed class AbsoluteDistance :
        IScatterSearchDistance<int>
    {
        public double Distance(
            in int left,
            in int right) =>
            Math.Abs(left - right);
    }

    private sealed class MinProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution) =>
            solution;
    }

    private sealed class QueueDiversification :
        IScatterSearchDiversificationGenerationMethod<int>
    {
        private readonly Queue<int> _values;

        public QueueDiversification(
            params int[] values) =>
            _values =
                new Queue<int>(values);

        public int Generate(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            _values.Dequeue();
    }
}
