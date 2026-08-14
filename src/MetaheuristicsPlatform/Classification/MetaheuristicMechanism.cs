namespace MetaheuristicsPlatform.Classification;

/// <summary>
/// Independent mechanisms used by a metaheuristic.
/// </summary>
[Flags]
public enum MetaheuristicMechanism
{
    None = 0,
    Neighborhood = 1 << 0,
    Trajectory = 1 << 1,
    EvolutionaryOperators = 1 << 2,
    Swarm = 1 << 3,
    Constructive = 1 << 4,
    MemoryBased = 1 << 5,
    Decomposition = 1 << 6,
    Adaptive = 1 << 7,
    Hybrid = 1 << 8
}