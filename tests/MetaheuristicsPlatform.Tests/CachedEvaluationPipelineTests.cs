using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Caching;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Evaluation.Instrumentation;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class CachedEvaluationPipelineTests
{
    [Fact]
    public void BaldwinianCache_HitDoesNotAliasCachedSolution()
    {
        int evaluations = 0;

        var inner =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    (solution, _) =>
                    {
                        Interlocked.Increment(
                            ref evaluations);

                        return solution.Value;
                    }),
                new EvaluationCharacteristics(
                    true,
                    EvaluationCostHint.Heavy,
                    EvaluationVariabilityHint.Uniform),
                improver:
                    new DelegateSolutionImprover<MutableSolution>(
                        static (solution, _) =>
                        {
                            solution.Value /= 2;
                            return true;
                        }),
                feedbackMode:
                    ImprovementFeedbackMode.Baldwinian);

        var cache =
            new ConcurrentEvaluationCache<
                int,
                EvaluationCacheEntry<int, MutableSolution>>();

        var cached =
            new CachedEvaluationPipeline<
                int,
                MutableSolution,
                int>(
                inner,
                new DelegateEvaluationCacheKeySelector<int, int>(
                    static candidate => candidate),
                cache,
                new DelegateEvaluationSnapshotCloner<MutableSolution>(
                    static solution =>
                        new MutableSolution(
                            solution.Value)));

        int firstCandidate = 10;

        EvaluationPipelineResult<MutableSolution> first =
            cached.Evaluate(
                ref firstCandidate,
                TestContext.Current.CancellationToken);

        first.Solution.Value = 999;

        int secondCandidate = 10;

        EvaluationPipelineResult<MutableSolution> second =
            cached.Evaluate(
                ref secondCandidate,
                TestContext.Current.CancellationToken);

        Assert.Equal(1, evaluations);
        Assert.Equal(5.0, second.Fitness);
        Assert.Equal(5, second.Solution.Value);
        Assert.NotSame(
            first.Solution,
            second.Solution);
        Assert.Equal(10, secondCandidate);
    }

    [Fact]
    public void LamarckianCache_HitReplaysCandidateFeedback()
    {
        int evaluations = 0;

        var inner =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    (solution, _) =>
                    {
                        Interlocked.Increment(
                            ref evaluations);

                        return solution.Value;
                    }),
                new EvaluationCharacteristics(
                    true,
                    EvaluationCostHint.Heavy,
                    EvaluationVariabilityHint.Uniform),
                improver:
                    new DelegateSolutionImprover<MutableSolution>(
                        static (solution, _) =>
                        {
                            solution.Value -= 3;
                            return true;
                        }),
                feedbackMode:
                    ImprovementFeedbackMode.Lamarckian,
                feedback:
                    new DelegateLamarckianFeedback<int, MutableSolution>(
                        static (
                            MutableSolution solution,
                            ref int candidate,
                            CancellationToken _) =>
                        {
                            candidate =
                                solution.Value;
                        }));

        var cached =
            new CachedEvaluationPipeline<
                int,
                MutableSolution,
                int>(
                inner,
                new DelegateEvaluationCacheKeySelector<int, int>(
                    static candidate => candidate),
                new ConcurrentEvaluationCache<
                    int,
                    EvaluationCacheEntry<int, MutableSolution>>(),
                new DelegateEvaluationSnapshotCloner<MutableSolution>(
                    static solution =>
                        new MutableSolution(
                            solution.Value)),
                new ImmutableEvaluationSnapshotCloner<int>());

        int firstCandidate = 10;

        cached.Evaluate(
            ref firstCandidate,
            TestContext.Current.CancellationToken);

        Assert.Equal(7, firstCandidate);

        int secondCandidate = 10;

        EvaluationPipelineResult<MutableSolution> second =
            cached.Evaluate(
                ref secondCandidate,
                TestContext.Current.CancellationToken);

        Assert.Equal(1, evaluations);
        Assert.Equal(7, secondCandidate);
        Assert.Equal(7.0, second.Fitness);
    }

    [Fact]
    public void CacheMetrics_DistinguishMissAndHit()
    {
        var metrics =
            new EvaluationPipelineMetrics();

        var inner =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    static (solution, _) =>
                        solution.Value),
                new EvaluationCharacteristics(false));

        var cached =
            new CachedEvaluationPipeline<
                int,
                MutableSolution,
                int>(
                inner,
                new DelegateEvaluationCacheKeySelector<int, int>(
                    static candidate => candidate),
                new ConcurrentEvaluationCache<
                    int,
                    EvaluationCacheEntry<int, MutableSolution>>(),
                new DelegateEvaluationSnapshotCloner<MutableSolution>(
                    static solution =>
                        new MutableSolution(
                            solution.Value)),
                metricsSink:
                    metrics);

        int candidate = 42;

        cached.Evaluate(
            ref candidate,
            TestContext.Current.CancellationToken);

        candidate = 42;

        cached.Evaluate(
            ref candidate,
            TestContext.Current.CancellationToken);

        EvaluationPipelineMetricsSnapshot snapshot =
            metrics.Snapshot();

        Assert.Equal(1, snapshot.CacheMissCount);
        Assert.Equal(1, snapshot.CacheHitCount);
        Assert.Equal(0.5, snapshot.CacheHitRatio);
    }

    [Fact]
    public void LamarckianCache_RequiresCandidateCloner()
    {
        var inner =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    static (solution, _) =>
                        solution.Value),
                new EvaluationCharacteristics(false),
                improver:
                    new DelegateSolutionImprover<MutableSolution>(
                        static (_, _) => false),
                feedbackMode:
                    ImprovementFeedbackMode.Lamarckian,
                feedback:
                    new DelegateLamarckianFeedback<int, MutableSolution>(
                        static (
                            MutableSolution solution,
                            ref int candidate,
                            CancellationToken _) =>
                        {
                            candidate =
                                solution.Value;
                        }));

        Assert.Throws<ArgumentException>(
            () =>
                new CachedEvaluationPipeline<
                    int,
                    MutableSolution,
                    int>(
                    inner,
                    new DelegateEvaluationCacheKeySelector<int, int>(
                        static candidate => candidate),
                    new ConcurrentEvaluationCache<
                        int,
                        EvaluationCacheEntry<int, MutableSolution>>(),
                    new DelegateEvaluationSnapshotCloner<MutableSolution>(
                        static solution =>
                            new MutableSolution(
                                solution.Value))));
    }

    private sealed class MutableSolution
    {
        internal MutableSolution(int value)
        {
            Value = value;
        }

        internal int Value { get; set; }
    }
}