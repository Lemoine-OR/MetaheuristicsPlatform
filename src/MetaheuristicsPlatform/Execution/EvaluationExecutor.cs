namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Executes independent candidate evaluation work.
/// Uses coarse ranges for homogeneous work and fine-grained scheduling for
/// expensive/high-variability evaluation.
/// </summary>
public static class EvaluationExecutor
{
    public static void ForCandidates(
        int candidateCount,
        int representationDimension,
        EvaluationCharacteristics characteristics,
        EvaluationExecutionOptions options,
        CandidateRangeAction action,
        CancellationToken cancellationToken = default)
    {
        if (candidateCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateCount));
        }

        if (representationDimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(representationDimension));
        }

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(action);

        int processors =
            Environment.ProcessorCount;

        bool parallel =
            EvaluationExecutionPolicy.ShouldParallelize(
                candidateCount,
                representationDimension,
                characteristics,
                options,
                processors);

        if (!parallel)
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(0, candidateCount);
            return;
        }

        var parallelOptions =
            new ParallelOptions
            {
                CancellationToken =
                    cancellationToken,
                MaxDegreeOfParallelism =
                    options.MaxDegreeOfParallelism
            };

        bool fineGrained =
            characteristics.VariabilityHint ==
                EvaluationVariabilityHint.High ||
            characteristics.CostHint is
                EvaluationCostHint.Heavy or
                EvaluationCostHint.VeryHeavy;

        if (fineGrained)
        {
            // Heavy/heterogeneous work: scheduling overhead is dominated by
            // evaluation cost; one-candidate work items improve load balance.
            Parallel.For(
                0,
                candidateCount,
                parallelOptions,
                candidate =>
                    action(
                        candidate,
                        candidate + 1));

            return;
        }

        int maxWorkers =
            options.MaxDegreeOfParallelism > 0
                ? Math.Min(
                    options.MaxDegreeOfParallelism,
                    candidateCount)
                : Math.Min(
                    processors,
                    candidateCount);

        int requestedRangeCount =
            Math.Min(
                candidateCount,
                checked(
                    maxWorkers *
                    options.RangesPerWorker));

        int rangeSize =
            Math.Max(
                1,
                (candidateCount +
                    requestedRangeCount - 1) /
                requestedRangeCount);

        int actualRangeCount =
            (candidateCount +
                rangeSize - 1) /
            rangeSize;

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
                        candidateCount,
                        start + rangeSize);

                action(start, end);
            });
    }
}