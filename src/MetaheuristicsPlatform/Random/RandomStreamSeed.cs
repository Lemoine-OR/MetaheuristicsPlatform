using System.Runtime.CompilerServices;

namespace MetaheuristicsPlatform.Random;

/// <summary>
/// Stable deterministic derivation of independent stream seeds from one run seed.
/// </summary>
public static class RandomStreamSeed
{
    /// <summary>
    /// Derives a deterministic 64-bit stream seed from a root seed and stream identifier.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Derive(ulong rootSeed, ulong streamId)
    {
        ulong value =
            rootSeed +
            0x9E3779B97F4A7C15UL *
            unchecked(streamId + 1UL);

        value =
            (value ^ (value >> 30)) *
            0xBF58476D1CE4E5B9UL;

        value =
            (value ^ (value >> 27)) *
            0x94D049BB133111EBUL;

        return value ^ (value >> 31);
    }
}