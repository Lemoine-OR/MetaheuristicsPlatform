using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class LShadeArchiveTrimTests
{
    [Fact]
    public void TrimToCountShrinksWithoutChangingStorageCapacity()
    {
        var archive =
            new DeExternalArchive(
                capacity: 10,
                dimension: 2);

        IRandomSource random =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(123UL);

        for (int i = 0;
             i < 10;
             i++)
        {
            double[] vector =
                new[]
                {
                    (double)i,
                    (double)(-i)
                };

            archive.Add(
                vector,
                random);
        }

        archive.TrimToCount(
            maxCount: 4,
            random);

        Assert.Equal(
            4,
            archive.Count);

        Assert.Equal(
            10,
            archive.Capacity);
    }
}