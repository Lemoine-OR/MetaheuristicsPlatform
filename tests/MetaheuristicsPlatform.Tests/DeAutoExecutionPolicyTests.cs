using MetaheuristicsPlatform.Algorithms.DE.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class DeAutoExecutionPolicyTests
{
    [Theory]
    [InlineData(16, 128, false)]
    [InlineData(32, 32, false)]
    [InlineData(40, 32, false)]
    [InlineData(48, 32, true)]
    [InlineData(56, 32, true)]
    [InlineData(64, 32, true)]
    [InlineData(80, 32, true)]
    [InlineData(32, 64, true)]
    [InlineData(128, 16, true)]
    public void ReferenceSixteenThreadPolicy_MatchesCalibration(
        int populationSize,
        int dimension,
        bool expected)
    {
        bool actual =
            DeAutoExecutionPolicy.ShouldParallelize(
                populationSize,
                dimension,
                processorCount: 16);

        Assert.Equal(
            expected,
            actual);
    }

    [Fact]
    public void ReferenceSixteenThreadThresholds_AreExpected()
    {
        Assert.Equal(
            32,
            DeAutoExecutionPolicy.GetMinimumPopulation(
                processorCount: 16));

        Assert.Equal(
            1536,
            DeAutoExecutionPolicy.GetMinimumWork(
                processorCount: 16));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    public void OneOrInvalidProcessorCount_DoesNotParallelize(
        int processorCount)
    {
        Assert.False(
            DeAutoExecutionPolicy.ShouldParallelize(
                populationSize: 256,
                dimension: 256,
                processorCount));
    }

    [Fact]
    public void ExplicitMinimumParallelWork_RemainsAnOverride()
    {
        var options =
            new DeExecutionOptions
            {
                Mode =
                    DeExecutionMode.Auto,
                MinimumParallelWork = 1
            };

        if (Environment.ProcessorCount > 1)
        {
            Assert.True(
                options.ShouldParallelize(
                    populationSize: 2,
                    dimension: 1));
        }
    }
}