using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.DE;

/// <summary>
/// Allocation-free distinct DE donor-index sampling.
/// </summary>
public static class DeDistinctIndexSampler
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sample3(
        IRandomSource random,
        int populationSize,
        int excluded,
        out int r1,
        out int r2,
        out int r3)
    {
        r1 =
            NextDifferent(
                random,
                populationSize,
                excluded);

        do
        {
            r2 =
                NextDifferent(
                    random,
                    populationSize,
                    excluded);
        }
        while (r2 == r1);

        do
        {
            r3 =
                NextDifferent(
                    random,
                    populationSize,
                    excluded);
        }
        while (r3 == r1 ||
               r3 == r2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sample5(
        IRandomSource random,
        int populationSize,
        int excluded,
        out int r1,
        out int r2,
        out int r3,
        out int r4,
        out int r5)
    {
        Sample3(
            random,
            populationSize,
            excluded,
            out r1,
            out r2,
            out r3);

        do
        {
            r4 =
                NextDifferent(
                    random,
                    populationSize,
                    excluded);
        }
        while (r4 == r1 ||
               r4 == r2 ||
               r4 == r3);

        do
        {
            r5 =
                NextDifferent(
                    random,
                    populationSize,
                    excluded);
        }
        while (r5 == r1 ||
               r5 == r2 ||
               r5 == r3 ||
               r5 == r4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int NextDifferent(
        IRandomSource random,
        int populationSize,
        int excluded)
    {
        int candidate;

        do
        {
            candidate =
                random.NextInt32(
                    populationSize);
        }
        while (candidate == excluded);

        return candidate;
    }
}