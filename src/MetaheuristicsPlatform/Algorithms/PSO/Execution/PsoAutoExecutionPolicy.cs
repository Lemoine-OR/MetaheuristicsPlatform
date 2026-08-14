namespace MetaheuristicsPlatform.Algorithms.PSO.Execution;

/// <summary>
/// CPU-scaled Auto policy for homogeneous PSO particle movement.
/// Calibrated from the first benchmark campaign and intentionally conservative.
/// </summary>
public static class PsoAutoExecutionPolicy
{
    public static bool ShouldParallelize(
        int particleCount,
        int dimension,
        int processorCount)
    {
        if (particleCount <= 1 ||
            dimension <= 0 ||
            processorCount <= 1)
        {
            return false;
        }

        long work =
            (long)particleCount *
            dimension;

        int minimumParticles =
            Math.Max(
                16,
                2 * processorCount);

        long minimumWork =
            Math.Max(
                1_024L,
                160L * processorCount);

        return
            particleCount >= minimumParticles &&
            work >= minimumWork;
    }
}