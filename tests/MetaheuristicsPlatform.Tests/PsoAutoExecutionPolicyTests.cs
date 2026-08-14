using MetaheuristicsPlatform.Algorithms.PSO.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoAutoExecutionPolicyTests
{
    [Fact]
    public void CalibratedPolicy_MatchesMeasuredCrossoverOnSixteenProcessors()
    {
        const int processors = 16;

        Assert.False(
            PsoAutoExecutionPolicy.ShouldParallelize(
                particleCount: 64,
                dimension: 32,
                processorCount: processors));

        Assert.True(
            PsoAutoExecutionPolicy.ShouldParallelize(
                particleCount: 80,
                dimension: 32,
                processorCount: processors));
    }

    [Fact]
    public void CalibratedPolicy_IsShapeAwareAtEqualWork()
    {
        const int processors = 16;

        Assert.True(
            PsoAutoExecutionPolicy.ShouldParallelize(
                particleCount: 32,
                dimension: 128,
                processorCount: processors));

        Assert.True(
            PsoAutoExecutionPolicy.ShouldParallelize(
                particleCount: 256,
                dimension: 16,
                processorCount: processors));
    }
}