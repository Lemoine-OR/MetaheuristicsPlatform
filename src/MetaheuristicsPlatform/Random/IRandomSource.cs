namespace MetaheuristicsPlatform.Random;

/// <summary>
/// High-performance deterministic pseudo-random source consumed by metaheuristics.
/// </summary>
public interface IRandomSource
{
    /// <summary>Gets the seed from which this source was initialized.</summary>
    ulong Seed { get; }

    /// <summary>Returns the next uniformly distributed 64-bit unsigned integer.</summary>
    ulong NextUInt64();

    /// <summary>Returns a uniformly distributed double in [0, 1).</summary>
    double NextDouble();

    /// <summary>Returns an integer in [0, <paramref name="exclusiveMax"/>).</summary>
    int NextInt32(int exclusiveMax);

    /// <summary>
    /// Returns an integer in [<paramref name="inclusiveMin"/>, <paramref name="exclusiveMax"/>).
    /// </summary>
    int NextInt32(int inclusiveMin, int exclusiveMax);

    /// <summary>Fills a byte buffer with pseudo-random bytes.</summary>
    void Fill(Span<byte> buffer);
}