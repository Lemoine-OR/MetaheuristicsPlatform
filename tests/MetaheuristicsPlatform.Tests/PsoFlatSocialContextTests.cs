using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.State;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoFlatSocialContextTests
{
    [Fact]
    public void FlatContext_ReadsParticleMajorBuffersWithoutCopy()
    {
        var buffers =
            new PsoSwarmBuffers(3, 2);

        buffers.GetPosition(1)
            .CopyFrom(new[] { 4.0, 5.0 });

        buffers.GetPersonalBestPosition(2)
            .CopyFrom(new[] { 8.0, 9.0 });

        buffers.PersonalBestFitness[0] = 3.0;
        buffers.PersonalBestFitness[1] = 2.0;
        buffers.PersonalBestFitness[2] = 1.0;

        var topology =
            new FullyConnectedTopology();

        NeighborhoodGraph graph =
            topology.CreateGraph(
                new PsoTopologyContext(
                    3,
                    0,
                    OptimizationSense.Minimize),
                new Xoshiro256StarStarRandomSource(1UL));

        var context =
            new PsoSocialContext(
                buffers.Positions,
                buffers.PersonalBestPositions,
                buffers.PersonalBestFitness,
                buffers.SwarmSize,
                buffers.Dimension,
                graph,
                OptimizationSense.Minimize);

        Assert.Equal(
            new[] { 4.0, 5.0 },
            context.GetPosition(1).ToArray());

        Assert.Equal(
            new[] { 8.0, 9.0 },
            context.GetPersonalBestPosition(2).ToArray());
    }
}

internal static class SpanTestExtensions
{
    public static void CopyFrom(
        this Span<double> destination,
        ReadOnlySpan<double> source) =>
        source.CopyTo(destination);
}