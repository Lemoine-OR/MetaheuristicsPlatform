namespace MetaheuristicsPlatform.Algorithms.PSO;

/// <summary>Lightweight PSO-specific state exposed to callbacks/stopping rules.</summary>
public readonly record struct PsoIterationState(
    int SwarmSize,
    int Dimension,
    string TopologyId,
    string InfluencePolicyId,
    string VelocityDynamicsId);