namespace MetaheuristicsPlatform.Algorithms.PSO.Execution;

/// <summary>
/// Performance controls for homogeneous PSO particle-level CPU execution.
/// This controls movement/social kernels, not objective evaluation.
/// </summary>
public sealed class PsoExecutionOptions
{
    public PsoExecutionMode Mode { get; init; } =
        PsoExecutionMode.Auto;

    public int MaxDegreeOfParallelism { get; init; } =
        -1;

    /// <summary>
    /// Optional explicit product threshold override for Auto.
    /// Zero uses the calibrated shape-aware CPU-scaled policy.
    /// </summary>
    public int MinimumParallelWork { get; init; } = 0;

    public int RangesPerWorker { get; init; } = 4;

    public void Validate()
    {
        if (MaxDegreeOfParallelism == 0 ||
            MaxDegreeOfParallelism < -1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxDegreeOfParallelism));
        }

        if (MinimumParallelWork < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumParallelWork));
        }

        if (RangesPerWorker <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RangesPerWorker));
        }
    }

    public bool ShouldParallelize(
        int particleCount,
        int dimension)
    {
        Validate();

        if (particleCount <= 1 ||
            Environment.ProcessorCount <= 1)
        {
            return false;
        }

        return Mode switch
        {
            PsoExecutionMode.Sequential => false,
            PsoExecutionMode.Parallel => true,
            PsoExecutionMode.Auto =>
                MinimumParallelWork > 0
                    ? (long)particleCount * dimension >=
                        MinimumParallelWork
                    : PsoAutoExecutionPolicy.ShouldParallelize(
                        particleCount,
                        dimension,
                        Environment.ProcessorCount),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}