using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Tests;

public sealed class ArraySolutionClonerTests
{
    [Fact]
    public void Clone_CreatesIndependentArray()
    {
        double[] original = { 1.0, 2.0, 3.0 };
        var cloner = new ArraySolutionCloner<double>();

        double[] clone = cloner.Clone(original);
        clone[0] = 99.0;

        Assert.Equal(1.0, original[0]);
        Assert.Equal(99.0, clone[0]);
        Assert.NotSame(original, clone);
    }
}