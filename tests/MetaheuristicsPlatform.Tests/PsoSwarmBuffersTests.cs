using MetaheuristicsPlatform.Algorithms.PSO.State;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoSwarmBuffersTests
{
    [Fact]
    public void ParticleViews_AreIndependentAndContiguous()
    {
        var buffers =
            new PsoSwarmBuffers(
                swarmSize: 3,
                dimension: 4);

        Span<double> first =
            buffers.GetPosition(0);

        Span<double> second =
            buffers.GetPosition(1);

        first.Fill(1.0);
        second.Fill(2.0);

        Assert.Equal(
            new[]
            {
                1.0, 1.0, 1.0, 1.0,
                2.0, 2.0, 2.0, 2.0
            },
            buffers.Positions[..8]);
    }

    [Fact]
    public void PersonalBestView_DoesNotAllocateSeparateParticleArray()
    {
        var buffers =
            new PsoSwarmBuffers(2, 3);

        Span<double> best =
            buffers.GetPersonalBestPosition(1);

        best[0] = 7.0;

        Assert.Equal(
            7.0,
            buffers.PersonalBestPositions[3]);
    }
}