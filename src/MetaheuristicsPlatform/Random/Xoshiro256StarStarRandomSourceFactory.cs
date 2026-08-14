namespace MetaheuristicsPlatform.Random;

/// <summary>
/// Creates Xoshiro256** random sources.
/// </summary>
public sealed class Xoshiro256StarStarRandomSourceFactory : IRandomSourceFactory
{
    private Xoshiro256StarStarRandomSourceFactory()
    {
    }

    /// <summary>Gets the shared stateless factory.</summary>
    public static Xoshiro256StarStarRandomSourceFactory Instance { get; } = new();

    /// <inheritdoc />
    public string Id => "xoshiro256starstar-splitmix64";

    /// <inheritdoc />
    public IRandomSource Create(ulong seed) => new Xoshiro256StarStarRandomSource(seed);
}