using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Random connected graph: a random spanning tree guarantees connectivity,
/// then additional undirected edges are sampled independently.
/// </summary>
public sealed class RandomConnectedTopology : IPsoTopology
{
    public RandomConnectedTopology(
        double extraEdgeProbability = 0.15,
        bool includeSelf = true)
    {
        if (extraEdgeProbability is < 0.0 or > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(extraEdgeProbability));
        }

        ExtraEdgeProbability = extraEdgeProbability;
        IncludeSelf = includeSelf;
    }

    public double ExtraEdgeProbability { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "random-connected",
        Name = "Random Connected",
        Dynamics = PsoTopologyDynamics.RandomStatic,
        IsPublishedExactVariant = false,
        Notes = "Generic connected random-graph implementation inspired by random population structures evaluated by Kennedy & Mendes (2002).",
        References = new[] { PsoTopologyReferences.KennedyMendes2002 }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        int n = context.SwarmSize;
        var builder = new UndirectedGraphBuilder(n);

        int[] order = Enumerable.Range(0, n).ToArray();
        Shuffle(order, random);

        for (int i = 1; i < n; i++)
        {
            int parent = order[random.NextInt32(i)];
            builder.AddEdge(order[i], parent);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (random.NextDouble() < ExtraEdgeProbability)
                {
                    builder.AddEdge(i, j);
                }
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }

    private static void Shuffle(int[] values, IRandomSource random)
    {
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = random.NextInt32(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}