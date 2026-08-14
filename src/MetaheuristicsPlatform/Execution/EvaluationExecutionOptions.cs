namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Generic execution controls for independent candidate evaluations.
/// Reusable by PSO, GA, DE, ACO, local-search populations and future hybrids.
/// </summary>
public sealed class EvaluationExecutionOptions
{
    public EvaluationExecutionMode Mode { get; init; } =
        EvaluationExecutionMode.Auto;

    /// <summary>
    /// -1 delegates worker count to .NET; positive values cap concurrency.
    /// </summary>
    public int MaxDegreeOfParallelism { get; init; } = -1;

    /// <summary>
    /// Number of coarse ranges per worker for low-variability evaluation.
    /// </summary>
    public int RangesPerWorker { get; init; } = 4;

    public void Validate()
    {
        if (MaxDegreeOfParallelism == 0 ||
            MaxDegreeOfParallelism < -1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxDegreeOfParallelism));
        }

        if (RangesPerWorker <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RangesPerWorker));
        }
    }
}