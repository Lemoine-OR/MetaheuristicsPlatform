using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;

public sealed class ContinuousSearchSpaceTests
{
    [Fact]
    public void Sample_IsReproducibleAndInsideBounds()
    {
        var space = new BoundedContinuousSearchSpace(
            new[] { -1.0, 10.0, 100.0 },
            new[] { 1.0, 20.0, 200.0 });

        var firstRandom = new Xoshiro256StarStarRandomSource(123UL);
        var secondRandom = new Xoshiro256StarStarRandomSource(123UL);

        double[] first = new double[3];
        double[] second = new double[3];

        space.Sample(firstRandom, first);
        space.Sample(secondRandom, second);

        Assert.Equal(first, second);
        Assert.True(space.Contains(first));
    }

    [Fact]
    public void Clamp_ClampsEveryCoordinate()
    {
        var space = BoundedContinuousSearchSpace.Uniform(3, -5.0, 5.0);
        double[] point = { -10.0, 2.0, 8.0 };

        space.Clamp(point);

        Assert.Equal(new[] { -5.0, 2.0, 5.0 }, point);
    }

    [Fact]
    public void Contains_ReturnsFalseForWrongDimension()
    {
        var space = BoundedContinuousSearchSpace.Uniform(3, -5.0, 5.0);

        Assert.False(space.Contains(new double[] { 0.0, 1.0 }));
    }

    [Fact]
    public void Constructor_RejectsInvalidBounds()
    {
        Assert.Throws<ArgumentException>(() =>
            new BoundedContinuousSearchSpace(
                new[] { 0.0 },
                new[] { 0.0 }));
    }
}