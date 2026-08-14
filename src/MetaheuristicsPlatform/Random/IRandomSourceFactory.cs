namespace MetaheuristicsPlatform.Random;

/// <summary>
/// Creates deterministic random sources from explicit seeds.
/// </summary>
public interface IRandomSourceFactory
{
    /// <summary>Gets a stable implementation identifier for experiment metadata.</summary>
    string Id { get; }

    /// <summary>Creates a new source initialized with <paramref name="seed"/>.</summary>
    IRandomSource Create(ulong seed);
}