namespace MetaheuristicsPlatform.Evaluation.Instrumentation;

/// <summary>
/// Power-of-two histogram of complete pipeline latency in raw Stopwatch ticks.
/// Bucket i contains values in approximately [2^i, 2^(i+1)) ticks.
/// </summary>
public sealed class EvaluationLatencyHistogramSnapshot
{
    internal EvaluationLatencyHistogramSnapshot(long[] buckets)
    {
        Buckets = buckets;
    }

    public IReadOnlyList<long> Buckets { get; }

    public long TotalObservations
    {
        get
        {
            long total = 0;

            for (int i = 0; i < Buckets.Count; i++)
            {
                total += Buckets[i];
            }

            return total;
        }
    }

    public TimeSpan GetApproximateUpperBound(int bucketIndex)
    {
        if ((uint)bucketIndex >= (uint)Buckets.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bucketIndex));
        }

        double upperTicks =
            Math.Pow(
                2.0,
                bucketIndex + 1);

        return TimeSpan.FromSeconds(
            upperTicks /
            System.Diagnostics.Stopwatch.Frequency);
    }
}