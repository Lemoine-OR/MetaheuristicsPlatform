using System.Buffers.Binary;
using System.Numerics;

namespace MetaheuristicsPlatform.Random;

/// <summary>
/// Xoshiro256** pseudo-random number generator by David Blackman and Sebastiano Vigna.
/// The 256-bit state is initialized using SplitMix64 from a 64-bit user seed.
/// </summary>
/// <remarks>
/// This generator is intended for high-performance non-cryptographic workloads.
/// </remarks>
public sealed class Xoshiro256StarStarRandomSource : IRandomSource
{
    private const double DoubleUnit = 1.0 / 9007199254740992.0; // 2^53

    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    /// <summary>Initializes the generator from a deterministic 64-bit seed.</summary>
    public Xoshiro256StarStarRandomSource(ulong seed)
    {
        Seed = seed;

        ulong splitMixState = seed;
        _s0 = NextSplitMix64(ref splitMixState);
        _s1 = NextSplitMix64(ref splitMixState);
        _s2 = NextSplitMix64(ref splitMixState);
        _s3 = NextSplitMix64(ref splitMixState);

        // Xoshiro's all-zero state is forbidden. SplitMix64 should not generate
        // four zero outputs here in practice, but keep the invariant explicit.
        if ((_s0 | _s1 | _s2 | _s3) == 0)
        {
            _s0 = 0x9E3779B97F4A7C15UL;
        }
    }

    /// <inheritdoc />
    public ulong Seed { get; }

    /// <inheritdoc />
    public ulong NextUInt64()
    {
        ulong result = BitOperations.RotateLeft(_s1 * 5UL, 7) * 9UL;
        ulong t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);

        return result;
    }

    /// <inheritdoc />
    public double NextDouble() =>
        (NextUInt64() >> 11) * DoubleUnit;

    /// <inheritdoc />
    public int NextInt32(int exclusiveMax)
    {
        if (exclusiveMax <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
        }

        return (int)NextUInt64Bounded((ulong)exclusiveMax);
    }

    /// <inheritdoc />
    public int NextInt32(int inclusiveMin, int exclusiveMax)
    {
        if (inclusiveMin >= exclusiveMax)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exclusiveMax),
                "exclusiveMax must be strictly greater than inclusiveMin.");
        }

        ulong range = (ulong)((long)exclusiveMax - inclusiveMin);
        long offset = (long)NextUInt64Bounded(range);
        return checked((int)(inclusiveMin + offset));
    }

    /// <inheritdoc />
    public void Fill(Span<byte> buffer)
    {
        int offset = 0;

        while (buffer.Length - offset >= sizeof(ulong))
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                buffer.Slice(offset, sizeof(ulong)),
                NextUInt64());
            offset += sizeof(ulong);
        }

        int remaining = buffer.Length - offset;
        if (remaining == 0)
        {
            return;
        }

        Span<byte> tail = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(tail, NextUInt64());
        tail[..remaining].CopyTo(buffer[offset..]);
    }

    private ulong NextUInt64Bounded(ulong exclusiveUpperBound)
    {
        // Rejection threshold removes modulo bias while preserving the full
        // 64-bit source range. `0 - bound` intentionally wraps in unsigned arithmetic.
        ulong threshold = unchecked(0UL - exclusiveUpperBound) % exclusiveUpperBound;

        while (true)
        {
            ulong value = NextUInt64();
            if (value >= threshold)
            {
                return value % exclusiveUpperBound;
            }
        }
    }

    private static ulong NextSplitMix64(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15UL;
        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}