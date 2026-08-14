using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Evaluation.Instrumentation;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationPipelineInstrumentationTests
{
    [Fact]
    public void InstrumentedPipeline_RecordsAllConfiguredStages()
    {
        var metrics =
            new EvaluationPipelineMetrics();

        var pipeline =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    static (solution, _) =>
                        solution.Value),
                new EvaluationCharacteristics(false),
                repair:
                    new DelegateSolutionRepair<MutableSolution>(
                        static (solution, _) =>
                        {
                            solution.Value++;
                            return true;
                        }),
                improver:
                    new DelegateSolutionImprover<MutableSolution>(
                        static (solution, _) =>
                        {
                            solution.Value++;
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
                        }),
                metricsSink:
                    metrics);

        int candidate = 5;

        EvaluationPipelineResult<MutableSolution> result =
            pipeline.Evaluate(
                ref candidate,
                TestContext.Current.CancellationToken);

        EvaluationPipelineMetricsSnapshot snapshot =
            metrics.Snapshot();

        Assert.Equal(7.0, result.Fitness);
        Assert.Equal(7, candidate);
        Assert.Equal(1, snapshot.EvaluationCount);
        Assert.Equal(1, snapshot.RepairCount);
        Assert.Equal(1, snapshot.ImprovementCount);
        Assert.Equal(1, snapshot.FeedbackCount);
        Assert.True(snapshot.TotalTicks > 0);
        Assert.True(snapshot.DecodeTicks >= 0);
        Assert.True(snapshot.RepairTicks >= 0);
        Assert.True(snapshot.ImproveTicks >= 0);
        Assert.True(snapshot.EvaluateTicks >= 0);
        Assert.True(snapshot.FeedbackTicks >= 0);
        Assert.Equal(
            1,
            metrics
                .GetTotalLatencyHistogram()
                .TotalObservations);
    }

    [Fact]
    public void Metrics_ResetClearsCounters()
    {
        var metrics =
            new EvaluationPipelineMetrics();

        metrics.RecordCacheHit();
        metrics.RecordCacheMiss();

        metrics.Reset();

        EvaluationPipelineMetricsSnapshot snapshot =
            metrics.Snapshot();

        Assert.Equal(0, snapshot.CacheHitCount);
        Assert.Equal(0, snapshot.CacheMissCount);
        Assert.Equal(0, snapshot.EvaluationCount);
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