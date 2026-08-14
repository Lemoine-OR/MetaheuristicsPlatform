using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Allocation-free random-distribution helpers for adaptive DE.
/// </summary>
public static class DeRandomDistributions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SampleNormal(
        IRandomSource random,
        double mean,
        double standardDeviation)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (!double.IsFinite(mean))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mean));
        }

        if (!double.IsFinite(standardDeviation) ||
            standardDeviation <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(standardDeviation));
        }

        double u1;

        do
        {
            u1 = random.NextDouble();
        }
        while (u1 <= double.Epsilon);

        double u2 =
            random.NextDouble();

        double radius =
            Math.Sqrt(
                -2.0 * Math.Log(u1));

        double z =
            radius *
            Math.Cos(
                2.0 * Math.PI * u2);

        return
            mean +
            standardDeviation * z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SampleCauchy(
        IRandomSource random,
        double location,
        double scale)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (!double.IsFinite(location))
        {
            throw new ArgumentOutOfRangeException(
                nameof(location));
        }

        if (!double.IsFinite(scale) ||
            scale <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale));
        }

        double u =
            random.NextDouble();

        return
            location +
            scale *
            Math.Tan(
                Math.PI * (u - 0.5));
    }
}