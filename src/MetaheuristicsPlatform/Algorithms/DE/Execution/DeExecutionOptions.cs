namespace MetaheuristicsPlatform.Algorithms.DE.Execution;

/// <summary>
/// Execution policy for homogeneous DE variation/selection kernels.
/// Objective evaluation uses the generic EvaluationExecutionOptions separately.
/// </summary>
public sealed class DeExecutionOptions
{
    public DeExecutionMode Mode { get; init; } =
        DeExecutionMode.Auto;

    public int MaxDegreeOfParallelism { get; init; } =
        -1;

    public int RangesPerWorker { get; init; } =
        4;

    /// <summary>
    /// Optional explicit scalar work override.
    /// Zero selects the calibrated DE-specific shape-aware Auto policy.
    /// A positive value makes Auto parallelize when
    /// populationSize * dimension reaches this value.
    /// </summary>
    public int MinimumParallelWork { get; init; } =
        0;

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

        if (MinimumParallelWork < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumParallelWork));
        }
    }

    public bool ShouldParallelize(
        int populationSize,
        int dimension)
    {
        Validate();

        if (populationSize <= 1 ||
            dimension <= 0 ||
            Environment.ProcessorCount <= 1)
        {
            return false;
        }

        return Mode switch
        {
            DeExecutionMode.Sequential =>
                false,

            DeExecutionMode.Parallel =>
                true,

            DeExecutionMode.Auto =>
                MinimumParallelWork > 0
                    ? (long)populationSize *
                        dimension >=
                        MinimumParallelWork
                    : DeAutoExecutionPolicy.ShouldParallelize(
                        populationSize,
                        dimension,
                        Environment.ProcessorCount),

            _ =>
                throw new ArgumentOutOfRangeException()
        };
    }
}