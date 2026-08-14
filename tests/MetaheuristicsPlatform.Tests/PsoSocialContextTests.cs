using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoSocialContextTests
{
    [Fact]
    public void Context_RejectsInconsistentDimensions()
    {
        double[][] positions =
        {
            new[] { 0.0, 0.0 },
            new[] { 1.0 }
        };

        double[][] personalBest =
        {
            new[] { 0.0, 0.0 },
            new[] { 1.0, 1.0 }
        };

        var topology =
            new FullyConnectedTopology();

        NeighborhoodGraph graph =
            topology.CreateGraph(
                new PsoTopologyContext(
                    2,
                    0,
                    OptimizationSense.Minimize),
                new Xoshiro256StarStarRandomSource(1UL));

        Assert.Throws<ArgumentException>(() =>
            new PsoSocialContext(
                positions,
                personalBest,
                new[] { 1.0, 2.0 },
                graph,
                OptimizationSense.Minimize));
    }
}