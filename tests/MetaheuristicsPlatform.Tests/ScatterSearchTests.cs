using MetaheuristicsPlatform.Algorithms.ScatterSearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class ScatterSearchTests
{
    [Fact]
    public void StableIdIsCanonicalAndCatalogReady()
    {
        var optimizer =
            CreateOptimizer(
                new QueueDiversification(0, 1, 5, 10),
                static subset =>
                    new[] { Average(subset) });

        Assert.Equal(
            "scatter-search-marti-laguna-glover-2006",
            optimizer.Descriptor.Id);

        Assert.Equal(
            "10.1016/j.ejor.2004.08.004",
            ScatterSearchReferences.MartiLagunaGlover2006.Doi);

        MetaheuristicCatalogEntry metadata =
            MetaheuristicCatalog.GetRequired(
                optimizer.Descriptor.Id);

        Assert.True(metadata.RequiresComposition);
    }

    [Fact]
    public void InitialReferenceSetBlendsQualityAndMaxMinDiversity()
    {
        var updater =
            new ClassicalScatterSearchReferenceSetUpdateMethod<int>();

        var population =
            new[]
            {
                Point(0),
                Point(1),
                Point(2),
                Point(10),
                Point(11)
            };

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>();

        updater.Initialize(
            referenceSet,
            population,
            referenceSetSize: 3,
            qualityReferenceSetSize: 2,
            new AbsoluteDistance(),
            OptimizationSense.Minimize,
            new ImmutableSolutionCloner<int>());

        Assert.Equal(3, referenceSet.Count);
        Assert.Contains(referenceSet, point => point.Solution == 0);
        Assert.Contains(referenceSet, point => point.Solution == 1);
        Assert.Contains(referenceSet, point => point.Solution == 11);
    }

    [Fact]
    public void PairwiseSubsetGenerationUsesAtLeastOneNewReferencePoint()
    {
        var generator =
            new PairwiseNewScatterSearchSubsetGenerationMethod<int>();

        var referenceSet =
            new[]
            {
                new ScatterSearchReferencePoint<int>(0, 0, false),
                new ScatterSearchReferencePoint<int>(1, 1, false),
                new ScatterSearchReferencePoint<int>(2, 2, true)
            };

        IReadOnlyList<ScatterSearchSubset<int>> subsets =
            generator.Generate(referenceSet);

        Assert.Equal(2, subsets.Count);
        Assert.All(
            subsets,
            subset =>
                Assert.Contains(
                    subset.Members,
                    member => member.Solution == 2));
    }

    [Fact]
    public void StrictlyBetterDistinctCandidateReplacesWorstReferencePoint()
    {
        var updater =
            new ClassicalScatterSearchReferenceSetUpdateMethod<int>();

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>
            {
                Point(1),
                Point(5),
                Point(9)
            };

        bool accepted =
            updater.TryUpdate(
                referenceSet,
                Point(3),
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.True(accepted);
        Assert.DoesNotContain(referenceSet, point => point.Solution == 9);
        Assert.Contains(referenceSet, point => point.Solution == 3);
    }

    [Fact]
    public void DuplicateCandidateIsRejectedEvenWhenObjectiveMatches()
    {
        var updater =
            new ClassicalScatterSearchReferenceSetUpdateMethod<int>();

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>
            {
                Point(1),
                Point(5),
                Point(9)
            };

        bool accepted =
            updater.TryUpdate(
                referenceSet,
                Point(1),
                new AbsoluteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>());

        Assert.False(accepted);
    }

    [Fact]
    public void ReferenceSetUpdateMirrorsMaximization()
    {
        var updater =
            new ClassicalScatterSearchReferenceSetUpdateMethod<int>();

        var referenceSet =
            new List<ScatterSearchReferencePoint<int>>
            {
                Point(1),
                Point(5),
                Point(9)
            };

        bool accepted =
            updater.TryUpdate(
                referenceSet,
                Point(7),
                new AbsoluteDistance(),
                OptimizationSense.Maximize,
                new ImmutableSolutionCloner<int>());

        Assert.True(accepted);
        Assert.DoesNotContain(referenceSet, point => point.Solution == 1);
        Assert.Contains(referenceSet, point => point.Solution == 7);
    }

    [Fact]
    public void OptimizerRunsDiversifyCombineImproveUpdateLifecycle()
    {
        var trace =
            new List<string>();

        var diversification =
            new TraceDiversification(
                trace,
                0, 2, 8, 10);

        var improvement =
            new TraceImprovement(trace);

        var combination =
            new DelegateScatterSearchSolutionCombinationMethod<int>(
                (subset, problem, random) =>
                {
                    trace.Add("combine");
                    return new[] { Average(subset) };
                });

        var optimizer =
            new ScatterSearchOptimizer<int>(
                diversification,
                improvement,
                new ClassicalScatterSearchReferenceSetUpdateMethod<int>(),
                new PairwiseNewScatterSearchSubsetGenerationMethod<int>(),
                combination,
                new AbsoluteDistance());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new ScatterSearchParameters
                {
                    DiversificationPopulationSize = 4,
                    ReferenceSetSize = 3,
                    QualityReferenceSetSize = 2,
                    MaximumIterations = 3
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Contains("diversify", trace);
        Assert.Contains("improve", trace);
        Assert.Contains("combine", trace);
        Assert.True(result.Statistics.Evaluations >= 4);
    }

    [Fact]
    public void StableReferenceSetTerminatesWithoutBurningMaximumIterations()
    {
        var optimizer =
            CreateOptimizer(
                new QueueDiversification(0, 2, 8, 10),
                static subset =>
                    new[] { 100 + Average(subset) });

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new ScatterSearchParameters
                {
                    DiversificationPopulationSize = 4,
                    ReferenceSetSize = 3,
                    QualityReferenceSetSize = 2,
                    MaximumIterations = 50
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "ReferenceSetStable",
            result.StopDecision.Criterion);

        Assert.True(result.Statistics.Iterations < 50);
    }

    [Fact]
    public void CombinationAliasingCannotMutateReferenceSetThroughImprovement()
    {
        var shared =
            new MutableBox(10);

        var diversification =
            new BoxDiversification(
                new MutableBox(0),
                new MutableBox(5),
                shared);

        var combination =
            new DelegateScatterSearchSolutionCombinationMethod<MutableBox>(
                (subset, problem, random) =>
                    new[] { subset.Members[0].Solution });

        int improvementCallCount = 0;

        var improvement =
            new DelegateScatterSearchImprovementMethod<MutableBox>(
                (ref MutableBox candidate,
                 IOptimizationProblem<MutableBox> problem,
                 IRandomSource random,
                 CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    improvementCallCount++;

                    // The first three calls improve the diversified population.
                    // Mutate only a later, combined candidate so this test isolates
                    // the combination-aliasing invariant.
                    if (improvementCallCount > 3)
                        candidate.Value = 1000;
                });

        var optimizer =
            new ScatterSearchOptimizer<MutableBox>(
                diversification,
                improvement,
                new ClassicalScatterSearchReferenceSetUpdateMethod<MutableBox>(),
                new PairwiseNewScatterSearchSubsetGenerationMethod<MutableBox>(),
                combination,
                new BoxDistance());

        _ = optimizer.Optimize(
            new BoxMinProblem(),
            new ScatterSearchParameters
            {
                DiversificationPopulationSize = 3,
                ReferenceSetSize = 2,
                QualityReferenceSetSize = 1,
                MaximumIterations = 1
            },
            new BoxCloner(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        // The original object handed to diversification must never become a RefSet alias.
        Assert.Equal(10, shared.Value);
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScatterSearchParameters
            {
                DiversificationPopulationSize = 1,
                ReferenceSetSize = 2,
                QualityReferenceSetSize = 1
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScatterSearchParameters
            {
                DiversificationPopulationSize = 10,
                ReferenceSetSize = 4,
                QualityReferenceSetSize = 4
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScatterSearchParameters
            {
                DiversificationPopulationSize = 10,
                ReferenceSetSize = 4,
                QualityReferenceSetSize = 2,
                MaximumIterations = 0
            }.Validate());
    }

    [Fact]
    public void NonFiniteDistanceIsRejected()
    {
        var updater =
            new ClassicalScatterSearchReferenceSetUpdateMethod<int>();

        var population =
            new[]
            {
                Point(0),
                Point(1),
                Point(2)
            };

        Assert.Throws<InvalidOperationException>(() =>
            updater.Initialize(
                new List<ScatterSearchReferencePoint<int>>(),
                population,
                2,
                1,
                new NonFiniteDistance(),
                OptimizationSense.Minimize,
                new ImmutableSolutionCloner<int>()));
    }

    private static ScatterSearchOptimizer<int> CreateOptimizer(
        IScatterSearchDiversificationGenerationMethod<int> diversification,
        Func<ScatterSearchSubset<int>,IEnumerable<int>> combination) =>
        new(
            diversification,
            new DelegateScatterSearchSolutionCombinationMethod<int>(
                (subset, problem, random) =>
                    combination(subset)),
            new AbsoluteDistance());

    private static ScatterSearchReferencePoint<int> Point(
        int value) =>
        new(
            value,
            value,
            true);

    private static int Average(
        ScatterSearchSubset<int> subset) =>
        (int)Math.Round(
            subset.Members.Average(
                static point => point.Solution),
            MidpointRounding.AwayFromZero);

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

    private sealed class TraceDiversification :
        IScatterSearchDiversificationGenerationMethod<int>
    {
        private readonly List<string> _trace;
        private readonly Queue<int> _values;

        public TraceDiversification(
            List<string> trace,
            params int[] values)
        {
            _trace = trace;
            _values =
                new Queue<int>(values);
        }

        public int Generate(
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add("diversify");
            return _values.Dequeue();
        }
    }

    private sealed class TraceImprovement :
        IScatterSearchImprovementMethod<int>
    {
        private readonly List<string> _trace;

        public TraceImprovement(
            List<string> trace) =>
            _trace = trace;

        public void Improve(
            ref int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _trace.Add("improve");
        }
    }

    private sealed class AbsoluteDistance :
        IScatterSearchDistance<int>
    {
        public double Distance(
            in int left,
            in int right) =>
            Math.Abs(left - right);
    }

    private sealed class NonFiniteDistance :
        IScatterSearchDistance<int>
    {
        public double Distance(
            in int left,
            in int right) =>
            double.NaN;
    }

    private sealed class MutableBox
    {
        public MutableBox(int value) =>
            Value = value;

        public int Value { get; set; }
    }

    private sealed class BoxMinProblem :
        IOptimizationProblem<MutableBox>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            MutableBox solution) =>
            solution.Value;
    }

    private sealed class BoxCloner :
        ISolutionCloner<MutableBox>
    {
        public MutableBox Clone(
            MutableBox solution) =>
            new(solution.Value);
    }

    private sealed class BoxDiversification :
        IScatterSearchDiversificationGenerationMethod<MutableBox>
    {
        private readonly Queue<MutableBox> _values;

        public BoxDiversification(
            params MutableBox[] values) =>
            _values =
                new Queue<MutableBox>(values);

        public MutableBox Generate(
            IOptimizationProblem<MutableBox> problem,
            IRandomSource random) =>
            _values.Dequeue();
    }

    private sealed class BoxDistance :
        IScatterSearchDistance<MutableBox>
    {
        public double Distance(
            in MutableBox left,
            in MutableBox right) =>
            Math.Abs(left.Value - right.Value);
    }
}
