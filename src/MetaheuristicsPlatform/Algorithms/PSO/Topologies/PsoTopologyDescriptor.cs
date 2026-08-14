using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Scientific and structural metadata for a PSO communication topology.
/// </summary>
public sealed record PsoTopologyDescriptor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();
    public required PsoTopologyDynamics Dynamics { get; init; }
    public PsoTopologyRequiredData RequiredData { get; init; }
    public bool IsPublishedExactVariant { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<ScientificReference> References { get; init; } =
        Array.Empty<ScientificReference>();
}