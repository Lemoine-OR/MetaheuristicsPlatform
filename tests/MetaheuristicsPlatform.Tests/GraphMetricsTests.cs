using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Tests;

public sealed class GraphMetricsTests
{
    [Fact]
    public void FullyConnectedGraph_HasExpectedMetrics()
    {
        var topology = new FullyConnectedTopology(includeSelf: true);
        var context = new PsoTopologyContext(
            5,
            0,
            OptimizationSense.Minimize);

        NeighborhoodGraph graph = topology.CreateGraph(
            context,
            new Xoshiro256StarStarRandomSource(1UL));

        GraphMetrics metrics = GraphMetricsCalculator.Compute(graph);

        Assert.Equal(1, metrics.ConnectedComponents);
        Assert.Equal(4, metrics.MinimumDegree);
        Assert.Equal(4, metrics.MaximumDegree);
        Assert.Equal(1.0, metrics.Density, 12);
        Assert.Equal(1, metrics.Diameter);
        Assert.Equal(1.0, metrics.AveragePathLength, 12);
        Assert.Equal(1.0, metrics.AverageClusteringCoefficient, 12);
    }
}