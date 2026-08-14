using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Uses a caller-provided immutable communication graph.
/// </summary>
public sealed class CustomGraphTopology : IPsoTopology
{
    private readonly NeighborhoodGraph _graph;

    public CustomGraphTopology(
        NeighborhoodGraph graph,
        string id = "custom-graph",
        string name = "Custom Graph")
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));

        Descriptor = new PsoTopologyDescriptor
        {
            Id = id,
            Name = name,
            Dynamics = PsoTopologyDynamics.Static,
            IsPublishedExactVariant = false,
            Notes = "User-supplied graph."
        };
    }

    public PsoTopologyDescriptor Descriptor { get; }

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        if (_graph.NodeCount != context.SwarmSize)
        {
            throw new ArgumentException(
                "Custom graph node count must equal swarm size.");
        }

        return _graph;
    }
}