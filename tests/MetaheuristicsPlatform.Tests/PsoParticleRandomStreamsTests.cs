using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoParticleRandomStreamsTests
{
    [Fact]
    public void SameRootSeed_ProducesSameParticleStreams()
    {
        var first =
            new PsoParticleRandomStreams(
                10,
                123456UL,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        var second =
            new PsoParticleRandomStreams(
                10,
                123456UL,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        for (int particle = 0;
             particle < 10;
             particle++)
        {
            for (int draw = 0;
                 draw < 20;
                 draw++)
            {
                Assert.Equal(
                    first.Get(particle).NextUInt64(),
                    second.Get(particle).NextUInt64());
            }
        }
    }

    [Fact]
    public void DifferentParticles_HaveDifferentStreams()
    {
        var streams =
            new PsoParticleRandomStreams(
                2,
                42UL,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        Assert.NotEqual(
            streams.Get(0).NextUInt64(),
            streams.Get(1).NextUInt64());
    }
}