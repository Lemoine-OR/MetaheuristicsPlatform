using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Generic runtime options shared by all metaheuristics.
/// Algorithm-specific parameters live in strongly typed parameter objects.
/// </summary>
public sealed class OptimizationOptions
{
    /// <summary>
    /// Gets or initializes the deterministic 64-bit seed used to create the run random source.
    /// </summary>
    public ulong Seed { get; init; }

    /// <summary>
    /// Gets or initializes the factory used to create the run random source.
    /// </summary>
    public IRandomSourceFactory RandomSourceFactory { get; init; } =
        Xoshiro256StarStarRandomSourceFactory.Instance;

    /// <summary>Gets or initializes which callback events may be emitted.</summary>
    public OptimizationCallbackEvents CallbackEvents { get; init; } =
        OptimizationCallbackEvents.Started |
        OptimizationCallbackEvents.BestImproved |
        OptimizationCallbackEvents.IterationCompleted |
        OptimizationCallbackEvents.Completed;

    /// <summary>
    /// Gets or initializes the iteration callback frequency. A value of 1 emits every completed iteration.
    /// </summary>
    public int IterationCallbackFrequency { get; init; } = 1;

    /// <summary>Validates common options.</summary>
    public void Validate()
    {
        if (RandomSourceFactory is null)
        {
            throw new ArgumentNullException(nameof(RandomSourceFactory));
        }

        if (IterationCallbackFrequency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(IterationCallbackFrequency),
                "Iteration callback frequency must be strictly positive.");
        }
    }
}