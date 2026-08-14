namespace MetaheuristicsPlatform.Classification;

/// <summary>
/// High-level scientific families. Flags allow hybrid and cross-family methods.
/// </summary>
[Flags]
public enum MetaheuristicFamily
{
    None = 0,
    TrajectoryBased = 1 << 0,
    LocalSearch = 1 << 1,
    Evolutionary = 1 << 2,
    SwarmIntelligence = 1 << 3,
    Constructive = 1 << 4,
    DecompositionBased = 1 << 5,
    Hybrid = 1 << 6,
    Other = 1 << 7
}