using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Execution;

/// <summary>
/// Deterministic particle-owned random streams.
/// Stream identity is independent of thread identity and scheduling.
/// </summary>
public sealed class PsoParticleRandomStreams
{
    private readonly IRandomSource[] _streams;

    public PsoParticleRandomStreams(
        int swarmSize,
        ulong rootSeed,
        IRandomSourceFactory factory)
    {
        if (swarmSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(swarmSize));
        }

        ArgumentNullException.ThrowIfNull(factory);

        RootSeed = rootSeed;
        FactoryId = factory.Id;

        _streams =
            new IRandomSource[swarmSize];

        for (int particle = 0;
             particle < swarmSize;
             particle++)
        {
            ulong streamSeed =
                RandomStreamSeed.Derive(
                    rootSeed,
                    (ulong)particle);

            _streams[particle] =
                factory.Create(streamSeed) ??
                throw new InvalidOperationException(
                    "Random-source factory returned null.");
        }
    }

    public ulong RootSeed { get; }
    public string FactoryId { get; }
    public int Count => _streams.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IRandomSource Get(int particle)
    {
        if ((uint)particle >=
            (uint)_streams.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(particle));
        }

        return _streams[particle];
    }
}