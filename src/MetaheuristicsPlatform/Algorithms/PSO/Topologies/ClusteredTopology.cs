using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Generic clustered topology: particles are split into contiguous cliques,
/// and cluster gateways are connected in a ring.
/// </summary>
/// <remarks>
/// This is a reusable generalized clustered graph. It is not presented as the exact
/// FourClusters adjacency matrix from Mendes et al. (2004).
/// </remarks>
public sealed class ClusteredTopology : IPsoTopology
{
    public ClusteredTopology(
        int clusterCount,
        int gatewaysPerAdjacentPair = 1,
        bool includeSelf = true)
    {
        if (clusterCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clusterCount));
        }

        if (gatewaysPerAdjacentPair <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gatewaysPerAdjacentPair));
        }

        ClusterCount = clusterCount;
        GatewaysPerAdjacentPair = gatewaysPerAdjacentPair;
        IncludeSelf = includeSelf;
    }

    public int ClusterCount { get; }
    public int GatewaysPerAdjacentPair { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "clustered-general",
        Name = "General Clustered",
        Aliases = new[] { "FourClusters-inspired generalized structure" },
        Dynamics = PsoTopologyDynamics.Static,
        IsPublishedExactVariant = false,
        Notes = "Generalized clique-and-gateway topology; exact DCluster is implemented separately.",
        References = new[] { PsoTopologyReferences.MendesKennedyNeves2004 }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        if (ClusterCount > context.SwarmSize)
        {
            throw new ArgumentException(
                "Cluster count cannot exceed swarm size.");
        }

        var builder = new UndirectedGraphBuilder(context.SwarmSize);
        List<int[]> clusters = BuildClusters(context.SwarmSize);

        foreach (int[] cluster in clusters)
        {
            builder.AddClique(cluster);
        }

        if (clusters.Count > 1)
        {
            for (int cluster = 0; cluster < clusters.Count; cluster++)
            {
                int next = (cluster + 1) % clusters.Count;
                int gatewayCount = Math.Min(
                    GatewaysPerAdjacentPair,
                    Math.Min(clusters[cluster].Length, clusters[next].Length));

                for (int gateway = 0; gateway < gatewayCount; gateway++)
                {
                    builder.AddEdge(
                        clusters[cluster][gateway],
                        clusters[next][gateway]);
                }
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }

    private List<int[]> BuildClusters(int swarmSize)
    {
        List<int[]> clusters = [];
        int baseSize = swarmSize / ClusterCount;
        int remainder = swarmSize % ClusterCount;
        int nextParticle = 0;

        for (int cluster = 0; cluster < ClusterCount; cluster++)
        {
            int size = baseSize + (cluster < remainder ? 1 : 0);
            int[] members = new int[size];

            for (int i = 0; i < size; i++)
            {
                members[i] = nextParticle++;
            }

            clusters.Add(members);
        }

        return clusters;
    }
}