using MetaheuristicsPlatform.Algorithms.DE;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class DeDistinctIndexSamplerTests
{
    [Fact]
    public void Sample5_ReturnsFiveDistinctIndicesExcludingTarget()
    {
        IRandomSource random =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(12345UL);

        for (int repeat = 0;
             repeat < 100;
             repeat++)
        {
            DeDistinctIndexSampler.Sample5(
                random,
                20,
                excluded: 7,
                out int r1,
                out int r2,
                out int r3,
                out int r4,
                out int r5);

            int[] values =
                new[]
                {
                    r1,
                    r2,
                    r3,
                    r4,
                    r5
                };

            Assert.DoesNotContain(
                7,
                values);

            Assert.Equal(
                5,
                values.Distinct().Count());
        }
    }
}