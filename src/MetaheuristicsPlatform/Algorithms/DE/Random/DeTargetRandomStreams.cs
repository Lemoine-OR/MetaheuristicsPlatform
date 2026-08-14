using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Random;

/// <summary>
/// Deterministic target-owned random streams.
/// Parallel scheduling therefore cannot change the random sequence of a target.
/// </summary>
public sealed class DeTargetRandomStreams
{
    private readonly IRandomSource[] _streams;

    public DeTargetRandomStreams(
        int populationSize,
        ulong rootSeed,
        IRandomSourceFactory factory)
    {
        if (populationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(populationSize));
        }

        ArgumentNullException.ThrowIfNull(factory);

        _streams =
            new IRandomSource[
                populationSize];

        for (int target = 0;
             target < populationSize;
             target++)
        {
            ulong streamSeed =
                RandomStreamSeed.Derive(
                    rootSeed,
                    0x4445000000000000UL +
                    (ulong)target);

            _streams[target] =
                factory.Create(
                    streamSeed) ??
                throw new InvalidOperationException(
                    "Random-source factory returned null.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IRandomSource Get(int target) =>
        _streams[target];
}