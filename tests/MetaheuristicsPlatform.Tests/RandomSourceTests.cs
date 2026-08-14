using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class RandomSourceTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var first = new Xoshiro256StarStarRandomSource(123456789UL);
        var second = new Xoshiro256StarStarRandomSource(123456789UL);

        for (int i = 0; i < 1000; i++)
        {
            Assert.Equal(first.NextUInt64(), second.NextUInt64());
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentInitialSequence()
    {
        var first = new Xoshiro256StarStarRandomSource(1UL);
        var second = new Xoshiro256StarStarRandomSource(2UL);

        Assert.NotEqual(first.NextUInt64(), second.NextUInt64());
    }

    [Fact]
    public void NextDouble_IsAlwaysInsideHalfOpenUnitInterval()
    {
        var random = new Xoshiro256StarStarRandomSource(42UL);

        for (int i = 0; i < 10000; i++)
        {
            double value = random.NextDouble();
            Assert.True(value >= 0.0);
            Assert.True(value < 1.0);
        }
    }

    [Fact]
    public void NextInt32_RespectsRequestedRange()
    {
        var random = new Xoshiro256StarStarRandomSource(42UL);

        for (int i = 0; i < 10000; i++)
        {
            int value = random.NextInt32(-17, 23);
            Assert.InRange(value, -17, 22);
        }
    }

    [Fact]
    public void Fill_IsDeterministic()
    {
        var first = new Xoshiro256StarStarRandomSource(99UL);
        var second = new Xoshiro256StarStarRandomSource(99UL);

        byte[] firstBytes = new byte[37];
        byte[] secondBytes = new byte[37];

        first.Fill(firstBytes);
        second.Fill(secondBytes);

        Assert.Equal(firstBytes, secondBytes);
    }
}