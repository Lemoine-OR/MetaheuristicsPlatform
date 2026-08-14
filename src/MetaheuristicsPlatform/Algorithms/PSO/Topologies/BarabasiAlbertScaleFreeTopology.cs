using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Static Barabasi-Albert-style preferential-attachment graph.
/// </summary>
/// <remarks>
/// This is a generic static scale-free topology. It is intentionally distinct from
/// Zhang and Yi's SFIPSO, which couples a modified scale-free topology with fitness,
/// spatial information and fully-informed social influence.
/// </remarks>
public sealed class BarabasiAlbertScaleFreeTopology : IPsoTopology
{
    public BarabasiAlbertScaleFreeTopology(
        int initialCliqueSize = 3,
        int edgesPerNewNode = 2,
        bool includeSelf = true)
    {
        if (initialCliqueSize < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCliqueSize));
        }

        if (edgesPerNewNode <= 0 ||
            edgesPerNewNode > initialCliqueSize)
        {
            throw new ArgumentOutOfRangeException(nameof(edgesPerNewNode));
        }

        InitialCliqueSize = initialCliqueSize;
        EdgesPerNewNode = edgesPerNewNode;
        IncludeSelf = includeSelf;
    }

    public int InitialCliqueSize { get; }
    public int EdgesPerNewNode { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "scale-free-barabasi-albert",
        Name = "Scale Free (Barabasi-Albert style)",
        Dynamics = PsoTopologyDynamics.RandomStatic,
        IsPublishedExactVariant = false,
        Notes = "Generic static preferential-attachment graph; not the exact self-organizing SFIPSO algorithm.",
        References = new[] { PsoTopologyReferences.ZhangYi2011 }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        int n = context.SwarmSize;

        if (InitialCliqueSize > n)
        {
            throw new ArgumentException(
                "Initial clique size cannot exceed swarm size.");
        }

        var builder = new UndirectedGraphBuilder(n);
        int[] degrees = new int[n];

        for (int i = 0; i < InitialCliqueSize; i++)
        {
            for (int j = i + 1; j < InitialCliqueSize; j++)
            {
                builder.AddEdge(i, j);
                degrees[i]++;
                degrees[j]++;
            }
        }

        for (int node = InitialCliqueSize; node < n; node++)
        {
            int targetCount = Math.Min(EdgesPerNewNode, node);
            HashSet<int> selected = [];

            while (selected.Count < targetCount)
            {
                int target = SelectPreferentialTarget(
                    degrees,
                    node,
                    selected,
                    random);

                selected.Add(target);
            }

            foreach (int target in selected)
            {
                builder.AddEdge(node, target);
                degrees[node]++;
                degrees[target]++;
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }

    private static int SelectPreferentialTarget(
        int[] degrees,
        int exclusiveUpperBound,
        HashSet<int> excluded,
        IRandomSource random)
    {
        long totalWeight = 0;

        for (int i = 0; i < exclusiveUpperBound; i++)
        {
            if (!excluded.Contains(i))
            {
                totalWeight += Math.Max(1, degrees[i]);
            }
        }

        double draw = random.NextDouble() * totalWeight;
        long cumulative = 0;

        for (int i = 0; i < exclusiveUpperBound; i++)
        {
            if (excluded.Contains(i))
            {
                continue;
            }

            cumulative += Math.Max(1, degrees[i]);
            if (draw < cumulative)
            {
                return i;
            }
        }

        for (int i = exclusiveUpperBound - 1; i >= 0; i--)
        {
            if (!excluded.Contains(i))
            {
                return i;
            }
        }

        throw new InvalidOperationException(
            "No preferential-attachment target is available.");
    }
}