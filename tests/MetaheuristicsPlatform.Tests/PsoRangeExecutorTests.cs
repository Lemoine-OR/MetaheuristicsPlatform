using MetaheuristicsPlatform.Algorithms.PSO.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoRangeExecutorTests
{
    [Fact]
    public void Sequential_VisitsEveryParticleExactlyOnce()
    {
        int[] visits = new int[100];

        PsoRangeExecutor.ForParticles(
            100,
            10,
            new PsoExecutionOptions
            {
                Mode = PsoExecutionMode.Sequential
            },
            (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    visits[i]++;
                }
            },
            TestContext.Current.CancellationToken);

        Assert.All(
            visits,
            static count => Assert.Equal(1, count));
    }

    [Fact]
    public void Parallel_VisitsEveryParticleExactlyOnce()
    {
        int[] visits = new int[1_000];

        PsoRangeExecutor.ForParticles(
            1_000,
            100,
            new PsoExecutionOptions
            {
                Mode = PsoExecutionMode.Parallel,
                MaxDegreeOfParallelism = 4
            },
            (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    Interlocked.Increment(
                        ref visits[i]);
                }
            },
            TestContext.Current.CancellationToken);

        Assert.All(
            visits,
            static count => Assert.Equal(1, count));
    }
}