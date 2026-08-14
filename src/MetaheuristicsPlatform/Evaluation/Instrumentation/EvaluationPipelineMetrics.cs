namespace MetaheuristicsPlatform.Evaluation.Instrumentation;

/// <summary>
/// Lock-free aggregate metrics sink for evaluation pipelines.
/// </summary>
public sealed class EvaluationPipelineMetrics :
    IEvaluationPipelineMetricsSink
{
    private const int HistogramBucketCount = 64;

    private long _evaluationCount;
    private long _repairCount;
    private long _improvementCount;
    private long _feedbackCount;
    private long _cacheHitCount;
    private long _cacheMissCount;

    private long _decodeTicks;
    private long _repairTicks;
    private long _improveTicks;
    private long _evaluateTicks;
    private long _feedbackTicks;
    private long _totalTicks;

    private readonly long[] _totalLatencyBuckets =
        new long[HistogramBucketCount];

    public void RecordEvaluation(
        in EvaluationPipelineMeasurement measurement)
    {
        Interlocked.Increment(
            ref _evaluationCount);

        if (measurement.WasRepaired)
        {
            Interlocked.Increment(
                ref _repairCount);
        }

        if (measurement.WasImproved)
        {
            Interlocked.Increment(
                ref _improvementCount);
        }

        if (measurement.FeedbackApplied)
        {
            Interlocked.Increment(
                ref _feedbackCount);
        }

        Interlocked.Add(
            ref _decodeTicks,
            measurement.DecodeTicks);

        Interlocked.Add(
            ref _repairTicks,
            measurement.RepairTicks);

        Interlocked.Add(
            ref _improveTicks,
            measurement.ImproveTicks);

        Interlocked.Add(
            ref _evaluateTicks,
            measurement.EvaluateTicks);

        Interlocked.Add(
            ref _feedbackTicks,
            measurement.FeedbackTicks);

        Interlocked.Add(
            ref _totalTicks,
            measurement.TotalTicks);

        int bucket =
            GetHistogramBucket(
                measurement.TotalTicks);

        Interlocked.Increment(
            ref _totalLatencyBuckets[bucket]);
    }

    public void RecordCacheHit() =>
        Interlocked.Increment(
            ref _cacheHitCount);

    public void RecordCacheMiss() =>
        Interlocked.Increment(
            ref _cacheMissCount);

    public EvaluationPipelineMetricsSnapshot Snapshot() =>
        new(
            Interlocked.Read(
                ref _evaluationCount),
            Interlocked.Read(
                ref _repairCount),
            Interlocked.Read(
                ref _improvementCount),
            Interlocked.Read(
                ref _feedbackCount),
            Interlocked.Read(
                ref _cacheHitCount),
            Interlocked.Read(
                ref _cacheMissCount),
            Interlocked.Read(
                ref _decodeTicks),
            Interlocked.Read(
                ref _repairTicks),
            Interlocked.Read(
                ref _improveTicks),
            Interlocked.Read(
                ref _evaluateTicks),
            Interlocked.Read(
                ref _feedbackTicks),
            Interlocked.Read(
                ref _totalTicks));

    public EvaluationLatencyHistogramSnapshot
        GetTotalLatencyHistogram()
    {
        long[] copy =
            new long[HistogramBucketCount];

        for (int i = 0;
             i < copy.Length;
             i++)
        {
            copy[i] =
                Interlocked.Read(
                    ref _totalLatencyBuckets[i]);
        }

        return new EvaluationLatencyHistogramSnapshot(
            copy);
    }

    public void Reset()
    {
        Interlocked.Exchange(
            ref _evaluationCount,
            0);

        Interlocked.Exchange(
            ref _repairCount,
            0);

        Interlocked.Exchange(
            ref _improvementCount,
            0);

        Interlocked.Exchange(
            ref _feedbackCount,
            0);

        Interlocked.Exchange(
            ref _cacheHitCount,
            0);

        Interlocked.Exchange(
            ref _cacheMissCount,
            0);

        Interlocked.Exchange(
            ref _decodeTicks,
            0);

        Interlocked.Exchange(
            ref _repairTicks,
            0);

        Interlocked.Exchange(
            ref _improveTicks,
            0);

        Interlocked.Exchange(
            ref _evaluateTicks,
            0);

        Interlocked.Exchange(
            ref _feedbackTicks,
            0);

        Interlocked.Exchange(
            ref _totalTicks,
            0);

        for (int i = 0;
             i < _totalLatencyBuckets.Length;
             i++)
        {
            Interlocked.Exchange(
                ref _totalLatencyBuckets[i],
                0);
        }
    }

    private static int GetHistogramBucket(
        long ticks)
    {
        ulong value =
            (ulong)Math.Max(
                1L,
                ticks);

        int bucket =
            System.Numerics.BitOperations.Log2(
                value);

        return Math.Min(
            HistogramBucketCount - 1,
            bucket);
    }
}