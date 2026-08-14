namespace MetaheuristicsPlatform.Parameters;

/// <summary>
/// Contract implemented by every strongly typed algorithm-specific parameter object.
/// </summary>
public interface IMetaheuristicParameters
{
    /// <summary>
    /// Validates the parameter set and throws an argument-related exception when invalid.
    /// </summary>
    void Validate();
}