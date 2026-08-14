namespace MetaheuristicsPlatform.Parameters;

/// <summary>
/// Parameter object for algorithms that require no algorithm-specific configuration.
/// </summary>
public sealed class NoMetaheuristicParameters : IMetaheuristicParameters
{
    private NoMetaheuristicParameters()
    {
    }

    /// <summary>Gets the shared parameterless configuration.</summary>
    public static NoMetaheuristicParameters Instance { get; } = new();

    /// <inheritdoc />
    public void Validate()
    {
    }
}