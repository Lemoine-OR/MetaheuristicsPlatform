using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Complete communication graph. This is the graph underlying classical global-best (gbest) PSO.
/// </summary>
public sealed class FullyConnectedTopology : IPsoTopology
{
    public FullyConnectedTopology(bool includeSelf = true)
    {
        IncludeSelf = includeSelf;
    }

    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "fully-connected",
        Name = "Fully Connected",
        Aliases = new[] { "gbest communication graph", "all" },
        Dynamics = PsoTopologyDynamics.Static,
        IsPublishedExactVariant = true,
        References = new[]
        {
            PsoTopologyReferences.KennedyEberhart1995,
            PsoTopologyReferences.KennedyMendes2002,
            PsoTopologyReferences.MendesKennedyNeves2004
        }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        var builder = new UndirectedGraphBuilder(context.SwarmSize);

        for (int i = 0; i < context.SwarmSize; i++)
        {
            for (int j = i + 1; j < context.SwarmSize; j++)
            {
                builder.AddEdge(i, j);
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }
}