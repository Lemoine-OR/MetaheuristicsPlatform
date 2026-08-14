namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// State required to construct or rebuild a PSO topology.
/// </summary>
[Flags]
public enum PsoTopologyRequiredData
{
    None = 0,
    CurrentFitness = 1 << 0,
    PersonalBestFitness = 1 << 1,
    Positions = 1 << 2,
    Stagnation = 1 << 3
}