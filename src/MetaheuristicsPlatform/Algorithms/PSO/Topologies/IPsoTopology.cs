using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Constructs the communication graph used by a PSO swarm.
/// This contract describes who can inform whom, not how neighbor information is aggregated.
/// </summary>
public interface IPsoTopology
{
    /// <summary>Gets scientific and structural topology metadata.</summary>
    PsoTopologyDescriptor Descriptor { get; }

    /// <summary>Constructs or rebuilds the topology for the supplied swarm state.</summary>
    NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random);
}