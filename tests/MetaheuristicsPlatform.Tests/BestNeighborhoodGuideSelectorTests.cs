using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class BestNeighborhoodGuideSelectorTests
{
    [Fact]
    public void Minimization_SelectsLowestPersonalBestFitness()
    {
        PsoSocialContext context = CreateContext(
            OptimizationSense.Minimize,
            new[] { 5.0, 2.0, 7.0 });

        int selected =
            BestNeighborhoodGuideSelector.Select(
                0,
                context);

        Assert.Equal(1, selected);
    }

    [Fact]
    public void Maximization_SelectsHighestPersonalBestFitness()
    {
        PsoSocialContext context = CreateContext(
            OptimizationSense.Maximize,
            new[] { 5.0, 2.0, 7.0 });

        int selected =
            BestNeighborhoodGuideSelector.Select(
                0,
                context);

        Assert.Equal(2, selected);
    }

    private static PsoSocialContext CreateContext(
        OptimizationSense sense,
        double[] fitness)
    {
        double[][] positions =
        {
            new[] { 0.0 },
            new[] { 1.0 },
            new[] { 2.0 }
        };

        double[][] personalBest =
        {
            new[] { 0.0 },
            new[] { 1.0 },
            new[] { 2.0 }
        };

        var topology =
            new FullyConnectedTopology(
                includeSelf: true);

        NeighborhoodGraph graph =
            topology.CreateGraph(
                new PsoTopologyContext(
                    3,
                    0,
                    sense),
                new Xoshiro256StarStarRandomSource(1UL));

        return new PsoSocialContext(
            positions,
            personalBest,
            fitness,
            graph,
            sense);
    }
}