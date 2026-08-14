namespace MetaheuristicsPlatform.Algorithms.DE.Execution;

/// <summary>
/// Calibrated shape-aware Auto policy for Differential Evolution variation.
///
/// Calibration reference:
/// Intel Core i7-6900K, 8 physical / 16 logical cores, .NET 10.
///
/// The policy intentionally requires both:
/// - enough independent target individuals to feed workers;
/// - enough total component work to amortize parallel scheduling.
///
/// This policy is DE-specific and must not be reused as a universal
/// population-metaheuristic threshold.
/// </summary>
public static class DeAutoExecutionPolicy
{
    public static bool ShouldParallelize(
        int populationSize,
        int dimension,
        int processorCount)
    {
        if (populationSize <= 1 ||
            dimension <= 0 ||
            processorCount <= 1)
        {
            return false;
        }

        int minimumPopulation =
            Math.Max(
                16,
                checked(
                    2 * processorCount));

        long minimumWork =
            Math.Max(
                768L,
                96L * processorCount);

        long work =
            (long)populationSize *
            dimension;

        return
            populationSize >= minimumPopulation &&
            work >= minimumWork;
    }

    public static int GetMinimumPopulation(
        int processorCount)
    {
        if (processorCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(processorCount));
        }

        return Math.Max(
            16,
            checked(
                2 * processorCount));
    }

    public static long GetMinimumWork(
        int processorCount)
    {
        if (processorCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(processorCount));
        }

        return Math.Max(
            768L,
            96L * processorCount);
    }
}