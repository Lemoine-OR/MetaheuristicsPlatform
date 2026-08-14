using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Trajectory;

namespace MetaheuristicsPlatform.Tests;

public sealed class TrajectoryObjectiveComparisonTests
{
    [Theory]
    [InlineData(
        OptimizationSense.Minimize,
        1.0,
        2.0,
        TrajectoryTransitionQuality.Improving)]
    [InlineData(
        OptimizationSense.Minimize,
        2.0,
        1.0,
        TrajectoryTransitionQuality.Worsening)]
    [InlineData(
        OptimizationSense.Maximize,
        2.0,
        1.0,
        TrajectoryTransitionQuality.Improving)]
    [InlineData(
        OptimizationSense.Maximize,
        1.0,
        2.0,
        TrajectoryTransitionQuality.Worsening)]
    [InlineData(
        OptimizationSense.Minimize,
        2.0,
        2.0,
        TrajectoryTransitionQuality.Equal)]
    [InlineData(
        OptimizationSense.Maximize,
        2.0,
        2.0,
        TrajectoryTransitionQuality.Equal)]
    public void ClassificationIsSenseAware(
        OptimizationSense sense,
        double candidate,
        double current,
        TrajectoryTransitionQuality expected)
    {
        Assert.Equal(
            expected,
            TrajectoryObjectiveComparison.Classify(
                sense,
                candidate,
                current));
    }
}