using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Exact regular DCluster topology proposed by El Dor et al.
/// The topology is rebuilt from current-fitness ranking.
/// </summary>
/// <remarks>
/// Reference:
/// A. El Dor, D. Lemoine, M. Clerc, P. Siarry, L. Deroussi, M. Gourgand,
/// "Dynamic cluster in particle swarm optimization algorithm",
/// Natural Computing 14(4), 655-672, 2015.
/// DOI: 10.1007/s11047-014-9465-2.
///
/// For cluster size p, the regular published construction uses p+1 clusters and
/// therefore requires swarm size N = p(p+1).
///
/// Particles are ranked from worst current fitness to best current fitness.
/// Contiguous groups of p ranked particles form cliques.
/// The first/worst group is the central cluster.
/// Its j-th particle is connected to the worst particle of outer cluster j.
/// </remarks>
public sealed class DClusterTopology : IPsoTopology
{
    public DClusterTopology(
        int clusterSize,
        bool includeSelf = true)
    {
        if (clusterSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clusterSize));
        }

        ClusterSize = clusterSize;
        IncludeSelf = includeSelf;
    }

    public int ClusterSize { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "dcluster-exact",
        Name = "DCluster",
        Aliases = new[] { "Dynamic Cluster" },
        Dynamics = PsoTopologyDynamics.FitnessDynamic,
        RequiredData = PsoTopologyRequiredData.CurrentFitness,
        IsPublishedExactVariant = true,
        Notes = "Exact regular construction. Swarm size must satisfy N = p(p+1), where p is cluster size.",
        References = new[] { PsoTopologyReferences.ElDorEtAl2015 }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        if (!context.HasCurrentFitness)
        {
            throw new InvalidOperationException(
                "DCluster requires current fitness values.");
        }

        int expectedSwarmSize =
            checked(ClusterSize * (ClusterSize + 1));

        if (context.SwarmSize != expectedSwarmSize)
        {
            throw new ArgumentException(
                $"Exact DCluster with cluster size {ClusterSize} requires swarm size " +
                $"{expectedSwarmSize} = p(p+1), but received {context.SwarmSize}.");
        }

        int[] ranked = Enumerable.Range(
            0,
            context.SwarmSize).ToArray();

        Array.Sort(
            ranked,
            (first, second) => CompareWorstFirst(
                context,
                first,
                second));

        var builder =
            new UndirectedGraphBuilder(context.SwarmSize);

        int numberOfClusters = ClusterSize + 1;

        for (int cluster = 0;
             cluster < numberOfClusters;
             cluster++)
        {
            ReadOnlySpan<int> members =
                ranked.AsSpan(
                    cluster * ClusterSize,
                    ClusterSize);

            builder.AddClique(members);
        }

        // The first ranked cluster is the central (worst) cluster.
        // Each central particle is linked to the worst member (first ranked
        // member) of one distinct outer cluster.
        for (int outer = 1;
             outer < numberOfClusters;
             outer++)
        {
            int centralParticle = ranked[outer - 1];
            int outerWorstParticle =
                ranked[outer * ClusterSize];

            builder.AddEdge(
                centralParticle,
                outerWorstParticle);
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(
            builder,
            IncludeSelf);

        return builder.Build();
    }

    private static int CompareWorstFirst(
        PsoTopologyContext context,
        int first,
        int second)
    {
        double firstFitness =
            context.GetCurrentFitness(first);

        double secondFitness =
            context.GetCurrentFitness(second);

        bool firstNaN = double.IsNaN(firstFitness);
        bool secondNaN = double.IsNaN(secondFitness);

        if (firstNaN || secondNaN)
        {
            if (firstNaN && secondNaN)
            {
                return first.CompareTo(second);
            }

            return firstNaN ? -1 : 1;
        }

        int comparison =
            firstFitness.CompareTo(secondFitness);

        // Worst first:
        // - minimization: largest objective first => reverse ascending compare
        // - maximization: smallest objective first => ordinary ascending compare
        if (context.Sense == OptimizationSense.Minimize)
        {
            comparison = -comparison;
        }

        return comparison != 0
            ? comparison
            : first.CompareTo(second);
    }
}