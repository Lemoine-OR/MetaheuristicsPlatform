using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoParallelDeterminismTests
{
    [Fact]
    public void ParticleOwnedRandomStreams_MakeSchedulingIrrelevant()
    {
        const int swarmSize = 256;
        const int dimension = 32;

        double[] sequential =
            Run(
                PsoExecutionMode.Sequential,
                swarmSize,
                dimension);

        double[] parallel =
            Run(
                PsoExecutionMode.Parallel,
                swarmSize,
                dimension);

        Assert.Equal(sequential, parallel);
    }

    private static double[] Run(
        PsoExecutionMode mode,
        int swarmSize,
        int dimension)
    {
        var streams =
            new PsoParticleRandomStreams(
                swarmSize,
                20260814UL,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        double[] result =
            new double[swarmSize];

        PsoRangeExecutor.ForParticles(
            swarmSize,
            dimension,
            new PsoExecutionOptions
            {
                Mode = mode,
                MaxDegreeOfParallelism = 4
            },
            (start, end) =>
            {
                for (int particle = start;
                     particle < end;
                     particle++)
                {
                    IRandomSource random =
                        streams.Get(particle);

                    double sum = 0.0;

                    for (int d = 0;
                         d < dimension;
                         d++)
                    {
                        sum += random.NextDouble();
                    }

                    result[particle] = sum;
                }
            },
            TestContext.Current.CancellationToken);

        return result;
    }
}