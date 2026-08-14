using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class JadeArchiveTests
{
    [Fact]
    public void IndexedArchiveReadReturnsOwnedSnapshot()
    {
        var archive =
            new DeExternalArchive(
                capacity: 4,
                dimension: 3);

        IRandomSource random =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(123UL);

        double[] source =
            new[] { 1.0, 2.0, 3.0 };

        archive.Add(
            source,
            random);

        source[0] = 999.0;

        Assert.Equal(
            new[] { 1.0, 2.0, 3.0 },
            archive.GetVectorReadOnly(0).ToArray());
    }

    [Fact]
    public void FullArchiveNeverExceedsConfiguredCapacity()
    {
        var archive =
            new DeExternalArchive(
                capacity: 3,
                dimension: 2);

        IRandomSource random =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(456UL);

        for (int i = 0;
             i < 100;
             i++)
        {
            double[] value =
                new[] { (double)i, -i };

            archive.Add(
                value,
                random);

            Assert.True(
                archive.Count <= 3);
        }
    }
}