namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Boundary handling options for JADE.
/// </summary>
public enum JadeBoundaryHandling
{
    /// <summary>
    /// Canonical JADE correction:
    /// if v_j is below the lower bound use (lower + x_i,j)/2;
    /// if v_j is above the upper bound use (upper + x_i,j)/2.
    /// </summary>
    MidpointToTarget = 0,

    Clamp = 1,

    Reflect = 2
}