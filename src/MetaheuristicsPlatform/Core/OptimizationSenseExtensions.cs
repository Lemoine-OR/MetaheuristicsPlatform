namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Provides comparison helpers consistent across all metaheuristics.
/// </summary>
public static class OptimizationSenseExtensions
{
    /// <summary>
    /// Returns whether <paramref name="candidate"/> improves on <paramref name="incumbent"/>.
    /// NaN candidates are never considered improvements.
    /// </summary>
    public static bool IsBetter(this OptimizationSense sense, double candidate, double incumbent)
    {
        if (double.IsNaN(candidate))
        {
            return false;
        }

        if (double.IsNaN(incumbent))
        {
            return true;
        }

        return sense switch
        {
            OptimizationSense.Minimize => candidate < incumbent,
            OptimizationSense.Maximize => candidate > incumbent,
            _ => throw new ArgumentOutOfRangeException(nameof(sense), sense, "Unsupported optimization sense.")
        };
    }

    /// <summary>
    /// Returns whether a target objective value has been reached.
    /// </summary>
    public static bool IsTargetReached(this OptimizationSense sense, double bestFitness, double targetFitness)
    {
        if (double.IsNaN(bestFitness) || double.IsNaN(targetFitness))
        {
            return false;
        }

        return sense switch
        {
            OptimizationSense.Minimize => bestFitness <= targetFitness,
            OptimizationSense.Maximize => bestFitness >= targetFitness,
            _ => throw new ArgumentOutOfRangeException(nameof(sense), sense, "Unsupported optimization sense.")
        };
    }

    /// <summary>
    /// Gets the worst possible initial best value for the specified sense.
    /// </summary>
    public static double WorstValue(this OptimizationSense sense) =>
        sense switch
        {
            OptimizationSense.Minimize => double.PositiveInfinity,
            OptimizationSense.Maximize => double.NegativeInfinity,
            _ => throw new ArgumentOutOfRangeException(nameof(sense), sense, "Unsupported optimization sense.")
        };
}