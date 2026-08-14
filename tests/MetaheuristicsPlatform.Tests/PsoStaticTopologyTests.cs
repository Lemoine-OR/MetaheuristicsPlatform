using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoStaticTopologyTests
{
    private static readonly IRandomSource Random =
        new Xoshiro256StarStarRandomSource(123UL);

    [Fact]
    public void FullyConnected_IncludesEveryParticleWhenSelfEnabled()
    {
        var topology = new FullyConnectedTopology();
        NeighborhoodGraph graph = topology.CreateGraph(
            Context(6),
            Random);

        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(6, graph.GetNeighborCount(i));
        }
    }

    [Fact]
    public void RingRadiusOne_HasLeftSelfRight()
    {
        var topology = new RingTopology(radius: 1, includeSelf: true);
        NeighborhoodGraph graph = topology.CreateGraph(
            Context(8),
            Random);

        Assert.Equal(
            new[] { 0, 1, 7 },
            graph.GetNeighbors(0).ToArray());
    }

    [Fact]
    public void HubAndSpoke_HasExpectedNeighborhoodSizes()
    {
        var topology = new HubAndSpokeTopology(
            hubIndex: 2,
            includeSelf: true);

        NeighborhoodGraph graph = topology.CreateGraph(
            Context(6),
            Random);

        Assert.Equal(6, graph.GetNeighborCount(2));
        Assert.Equal(2, graph.GetNeighborCount(0));
        Assert.True(graph.ContainsEdge(0, 2));
    }

    [Fact]
    public void VonNeumannFourByFive_HasFourStructuralNeighbors()
    {
        var topology = new ToroidalVonNeumannTopology(
            rows: 4,
            columns: 5,
            includeSelf: true);

        NeighborhoodGraph graph = topology.CreateGraph(
            Context(20),
            Random);

        for (int i = 0; i < 20; i++)
        {
            Assert.Equal(5, graph.GetNeighborCount(i));
        }
    }

    [Fact]
    public void RandomConnected_IsAlwaysConnected()
    {
        var topology = new RandomConnectedTopology(
            extraEdgeProbability: 0.0,
            includeSelf: false);

        NeighborhoodGraph graph = topology.CreateGraph(
            Context(30),
            new Xoshiro256StarStarRandomSource(42UL));

        GraphMetrics metrics = GraphMetricsCalculator.Compute(graph);

        Assert.Equal(1, metrics.ConnectedComponents);
        Assert.Equal(29, graph.EdgeCount);
    }

    [Fact]
    public void ScaleFree_IsConnected()
    {
        var topology = new BarabasiAlbertScaleFreeTopology(
            initialCliqueSize: 3,
            edgesPerNewNode: 2,
            includeSelf: false);

        NeighborhoodGraph graph = topology.CreateGraph(
            Context(50),
            new Xoshiro256StarStarRandomSource(42UL));

        GraphMetrics metrics = GraphMetricsCalculator.Compute(graph);

        Assert.Equal(1, metrics.ConnectedComponents);
    }

    [Fact]
    public void SmallWorld_PreservesNodeCountAndSelfNeighborhood()
    {
        var topology = new WattsStrogatzSmallWorldTopology(
            neighborhoodSize: 4,
            rewiringProbability: 0.25,
            includeSelf: true);

        NeighborhoodGraph graph = topology.CreateGraph(
            Context(30),
            new Xoshiro256StarStarRandomSource(42UL));

        Assert.Equal(30, graph.NodeCount);

        for (int i = 0; i < 30; i++)
        {
            Assert.True(graph.ContainsEdge(i, i));
        }
    }

    private static PsoTopologyContext Context(int swarmSize) =>
        new(
            swarmSize,
            iteration: 0,
            OptimizationSense.Minimize);
}