using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>Scientific metadata for a PSO attraction/influence policy.</summary>
public sealed record PsoInfluenceDescriptor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool UsesOwnPersonalBest { get; init; }
    public bool UsesSingleNeighborhoodGuide { get; init; }
    public bool UsesAllInformers { get; init; }
    public bool IsPublishedExactStructure { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<ScientificReference> References { get; init; } =
        Array.Empty<ScientificReference>();
}