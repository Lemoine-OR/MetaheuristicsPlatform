namespace MetaheuristicsPlatform.Algorithms.DE.Execution;

public static class DeRangeExecutor
{
    public static void ForTargets(
        int populationSize,
        int dimension,
        DeExecutionOptions options,
        DeRangeAction action,
        CancellationToken cancellationToken = default)
    {
        if (populationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(populationSize));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(action);

        if (!options.ShouldParallelize(
                populationSize,
                dimension))
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(0, populationSize);
            return;
        }

        int processors =
            options.MaxDegreeOfParallelism > 0
                ? Math.Min(
                    options.MaxDegreeOfParallelism,
                    populationSize)
                : Math.Min(
                    Environment.ProcessorCount,
                    populationSize);

        int requestedRanges =
            Math.Min(
                populationSize,
                checked(
                    processors *
                    options.RangesPerWorker));

        int rangeSize =
            Math.Max(
                1,
                (populationSize +
                    requestedRanges - 1) /
                requestedRanges);

        int rangeCount =
            (populationSize +
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

        Parallel.For(
            0,
            rangeCount,
            parallelOptions,
            rangeIndex =>
            {
                int start =
                    rangeIndex *
                    rangeSize;

                int end =
                    Math.Min(
                        populationSize,
                        start + rangeSize);

                action(start, end);
            });
    }
}