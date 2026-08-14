using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// One central hub connected to every other particle.
/// Kennedy and Mendes (2002) call this structure "star".
/// </summary>
public sealed class HubAndSpokeTopology : IPsoTopology
{
    public HubAndSpokeTopology(int hubIndex = 0, bool includeSelf = true)
    {
        if (hubIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hubIndex));
        }

        HubIndex = hubIndex;
        IncludeSelf = includeSelf;
    }

    public int HubIndex { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "hub-and-spoke",
        Name = "Hub-and-Spoke",
        Aliases = new[] { "star (Kennedy & Mendes 2002)" },
        Dynamics = PsoTopologyDynamics.Static,
        IsPublishedExactVariant = true,
        Notes = "Named structurally to avoid the literature ambiguity where 'star' is sometimes used for gbest.",
        References = new[] { PsoTopologyReferences.KennedyMendes2002 }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        if (HubIndex >= context.SwarmSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(HubIndex),
                "Hub index must be smaller than swarm size.");
        }

        var builder = new UndirectedGraphBuilder(context.SwarmSize);

        for (int i = 0; i < context.SwarmSize; i++)
        {
            if (i != HubIndex)
            {
                builder.AddEdge(HubIndex, i);
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }
}