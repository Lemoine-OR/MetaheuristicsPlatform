using System.Runtime.CompilerServices;

namespace MetaheuristicsPlatform.Algorithms.PSO.Execution;

/// <summary>
/// Executes independent particle work sequentially or in coarse contiguous ranges.
/// </summary>
public static class PsoRangeExecutor
{
    public static void ForParticles(
        int particleCount,
        int dimension,
        PsoExecutionOptions options,
        PsoParticleRangeAction action,
        CancellationToken cancellationToken = default)
    {
        if (particleCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(particleCount));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(action);

        if (!options.ShouldParallelize(
                particleCount,
                dimension))
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(0, particleCount);
            return;
        }

        int maxWorkers =
            options.MaxDegreeOfParallelism > 0
                ? Math.Min(
                    options.MaxDegreeOfParallelism,
                    particleCount)
                : Math.Min(
                    Environment.ProcessorCount,
                    particleCount);

        int requestedRangeCount =
            Math.Min(
                particleCount,
                checked(
                    maxWorkers *
                    options.RangesPerWorker));

        int rangeSize =
            Math.Max(
                1,
                (particleCount +
                    requestedRangeCount - 1) /
                requestedRangeCount);

        int actualRangeCount =
            (particleCount +
                rangeSize - 1) /
            rangeSize;

        var parallelOptions =
            new ParallelOptions
            {
                CancellationToken =
                    cancellationToken,
                MaxDegreeOfParallelism =
                    options.MaxDegreeOfParallelism
            };

        // Integer range IDs avoid Partitioner<Tuple<int,int>> and per-range Tuple objects.
        Parallel.For(
            0,
            actualRangeCount,
            parallelOptions,
            rangeIndex =>
            {
                int start =
                    rangeIndex *
                    rangeSize;

                int end =
                    Math.Min(
                        particleCount,
                        start + rangeSize);

                action(start, end);
            });
    }

    /// <summary>
    /// Tight sequential range helper used by kernels and benchmarks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sequential(
        int startInclusive,
        int endExclusive,
        Action<int> particleAction)
    {
        for (int particle = startInclusive;
             particle < endExclusive;
             particle++)
        {
            particleAction(particle);
        }
    }
}