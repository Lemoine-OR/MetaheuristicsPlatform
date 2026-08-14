namespace MetaheuristicsPlatform.Classification;

/// <summary>
/// Multidimensional metadata describing a metaheuristic without imposing an inheritance hierarchy.
/// </summary>
public sealed record MetaheuristicDescriptor
{
    /// <summary>Stable machine-readable identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable name.</summary>
    public required string Name { get; init; }

    /// <summary>Common acronym when one exists.</summary>
    public string? Acronym { get; init; }

    /// <summary>Primary solution multiplicity model.</summary>
    public required MetaheuristicSolutionModel SolutionModel { get; init; }

    /// <summary>Scientific families to which the method belongs.</summary>
    public MetaheuristicFamily Families { get; init; }

    /// <summary>Algorithmic mechanisms used by the method.</summary>
    public MetaheuristicMechanism Mechanisms { get; init; }

    /// <summary>Search spaces supported by this implementation.</summary>
    public SearchSpaceKind SearchSpaces { get; init; }

    /// <summary>Whether stochastic decisions are intrinsic to the method.</summary>
    public bool IsStochastic { get; init; }

    /// <summary>Scientific references used by this implementation.</summary>
    public IReadOnlyList<ScientificReference> References { get; init; } =
        Array.Empty<ScientificReference>();
}