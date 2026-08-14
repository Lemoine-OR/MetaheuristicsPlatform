using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Circular local-best topology with a configurable radius.
/// Radius 1 gives the classical two adjacent neighbors.
/// </summary>
public sealed class RingTopology : IPsoTopology
{
    public RingTopology(int radius = 1, bool includeSelf = true)
    {
        if (radius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }

        Radius = radius;
        IncludeSelf = includeSelf;
    }

    public int Radius { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "ring",
        Name = "Ring",
        Aliases = new[] { "lbest ring", "circle" },
        Dynamics = PsoTopologyDynamics.Static,
        IsPublishedExactVariant = true,
        References = new[]
        {
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

        int n = context.SwarmSize;
        var builder = new UndirectedGraphBuilder(n);

        for (int i = 0; i < n; i++)
        {
            for (int d = 1; d <= Radius; d++)
            {
                builder.AddEdge(i, Mod(i - d, n));
                builder.AddEdge(i, Mod(i + d, n));
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }

    private static int Mod(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}