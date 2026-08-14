namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>Describes how a PSO communication topology evolves.</summary>
public enum PsoTopologyDynamics
{
    Static = 0,
    RandomStatic = 1,
    DynamicRandom = 2,
    FitnessDynamic = 3,
    SpatialDynamic = 4,
    AdaptiveDynamic = 5,
    SelfOrganizing = 6
}