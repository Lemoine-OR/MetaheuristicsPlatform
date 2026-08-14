using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Tests;

public sealed class OptimizationSenseTests
{
    [Fact]
    public void Minimize_RecognizesStrictImprovement()
    {
        Assert.True(OptimizationSense.Minimize.IsBetter(4.0, 5.0));
        Assert.False(OptimizationSense.Minimize.IsBetter(5.0, 5.0));
    }

    [Fact]
    public void Maximize_RecognizesStrictImprovement()
    {
        Assert.True(OptimizationSense.Maximize.IsBetter(6.0, 5.0));
        Assert.False(OptimizationSense.Maximize.IsBetter(5.0, 5.0));
    }

    [Fact]
    public void NaNCandidate_IsNeverBetter()
    {
        Assert.False(OptimizationSense.Minimize.IsBetter(double.NaN, 1.0));
        Assert.False(OptimizationSense.Maximize.IsBetter(double.NaN, 1.0));
    }
}