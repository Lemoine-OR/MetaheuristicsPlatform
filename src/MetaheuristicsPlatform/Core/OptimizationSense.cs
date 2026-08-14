namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Specifies whether an optimization problem is minimized or maximized.
/// </summary>
public enum OptimizationSense
{
    /// <summary>Lower objective values are better.</summary>
    Minimize = 0,

    /// <summary>Higher objective values are better.</summary>
    Maximize = 1
}