namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>Boundary treatment after a PSO position update.</summary>
public enum PsoBoundaryHandling
{
    None = 0,
    Clamp = 1,
    ClampAndZeroVelocity = 2,
    Reflect = 3
}