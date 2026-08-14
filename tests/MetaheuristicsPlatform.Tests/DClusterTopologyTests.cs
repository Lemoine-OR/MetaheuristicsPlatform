using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class DClusterTopologyTests
{
    [Fact]
    public void Minimization_ReproducesRegularFourByFiveConstruction()
    {
        // p = 4 => N = 4 * 5 = 20.
        // Fitness equals particle index; minimization therefore ranks
        // 19,18,...,0 from worst to best.
        double[] fitness = Enumerable.Range(0, 20)
            .Select(static value => (double)value)
            .ToArray();

        var topology = new DClusterTopology(
            clusterSize: 4,
            includeSelf: false);

        var context = new PsoTopologyContext(
            20,
            iteration: 7,
            OptimizationSense.Minimize,
            currentFitness: fitness);

        NeighborhoodGraph graph = topology.CreateGraph(
            context,
            new Xoshiro256StarStarRandomSource(1UL));

        // Central worst cluster: 19,18,17,16 is a clique.
        Assert.True(graph.ContainsEdge(19, 18));
        Assert.True(graph.ContainsEdge(19, 16));
        Assert.True(graph.ContainsEdge(17, 16));

        // Outer-cluster gateway edges:
        // central[0]->worst of outer 1 = 19->15
        // central[1]->worst of outer 2 = 18->11
        // central[2]->worst of outer 3 = 17->7
        // central[3]->worst of outer 4 = 16->3
        Assert.True(graph.ContainsEdge(19, 15));
        Assert.True(graph.ContainsEdge(18, 11));
        Assert.True(graph.ContainsEdge(17, 7));
        Assert.True(graph.ContainsEdge(16, 3));

        Assert.False(graph.ContainsEdge(19, 11));
    }

    [Fact]
    public void Maximization_RanksLowFitnessAsWorst()
    {
        double[] fitness = Enumerable.Range(0, 20)
            .Select(static value => (double)value)
            .ToArray();

        var topology = new DClusterTopology(
            clusterSize: 4,
            includeSelf: false);

        var context = new PsoTopologyContext(
            20,
            iteration: 1,
            OptimizationSense.Maximize,
            currentFitness: fitness);

        NeighborhoodGraph graph = topology.CreateGraph(
            context,
            new Xoshiro256StarStarRandomSource(1UL));

        // Worst central cluster for maximization is 0,1,2,3.
        Assert.True(graph.ContainsEdge(0, 1));
        Assert.True(graph.ContainsEdge(0, 4));
        Assert.True(graph.ContainsEdge(1, 8));
        Assert.True(graph.ContainsEdge(2, 12));
        Assert.True(graph.ContainsEdge(3, 16));
    }

    [Fact]
    public void ExactVariant_RejectsNonRegularSwarmSize()
    {
        double[] fitness = new double[21];

        var topology = new DClusterTopology(clusterSize: 4);

        var context = new PsoTopologyContext(
            21,
            iteration: 0,
            OptimizationSense.Minimize,
            currentFitness: fitness);

        Assert.Throws<ArgumentException>(() =>
            topology.CreateGraph(
                context,
                new Xoshiro256StarStarRandomSource(1UL)));
    }

    [Fact]
    public void Descriptor_ContainsExactPublicationDoi()
    {
        var topology = new DClusterTopology(clusterSize: 4);

        Assert.True(topology.Descriptor.IsPublishedExactVariant);

        Assert.Contains(
            topology.Descriptor.References,
            reference =>
                reference.Doi == "10.1007/s11047-014-9465-2");
    }
}