namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Canonical runtime catalog for executable Tabu Search memory and control components.
/// </summary>
public static class TabuSearchComponentCatalog
{
    private static readonly TabuSearchComponentDescriptor[] Entries =
    [
        new()
        {
            Id = TabuSearchComponentIds.ShortTermExpirationMemory,
            Name = "Expiration-based short-term tabu memory",
            Category = "memory",
            ImplementationType = "ExpirationTabuMemory<TAttribute>",
            Reference = "Glover (1989, 1990)",
            ScientificScope =
                "Expected O(1) tabu lookup plus ordered expiration for variable tenure."
        },
        new()
        {
            Id = TabuSearchComponentIds.AttributeFrequencyMemory,
            Name = "Attribute frequency memory",
            Category = "memory",
            ImplementationType = "AttributeFrequencyMemory<TAttribute>",
            Reference = "Glover (1989); Glover & Laguna (1997)",
            ScientificScope =
                "Long-term visit counts used by frequency-guided diversification."
        },
        new()
        {
            Id = TabuSearchComponentIds.ConfigurationRepetitionHashMemory,
            Name = "Configuration repetition hash memory",
            Category = "memory",
            ImplementationType = "ConfigurationRepetitionMemory",
            Reference = "Battiti & Tecchiolli (1994)",
            ScientificScope =
                "Expected O(1) detection of repeated configuration signatures and cycle lengths."
        },
        new()
        {
            Id = TabuSearchComponentIds.FixedTenure,
            Name = "Fixed tabu tenure",
            Category = "tenure",
            ImplementationType = "FixedTabuTenurePolicy",
            Reference = "Glover (1989)",
            ScientificScope =
                "Classical fixed short-term prohibition period."
        },
        new()
        {
            Id = TabuSearchComponentIds.UniformRandomTenure,
            Name = "Uniformly varying tabu tenure",
            Category = "tenure",
            ImplementationType = "UniformRandomTabuTenurePolicy",
            Reference = "Glover (1990)",
            ScientificScope =
                "Randomly varying tenure supported by the short-term TS foundation."
        },
        new()
        {
            Id = TabuSearchComponentIds.ReactiveTenure,
            Name = "Reactive tabu tenure",
            Category = "tenure",
            ImplementationType = "ReactiveTabuTenurePolicy",
            Reference = "Battiti & Tecchiolli (1994)",
            ScientificScope =
                "Feedback increases tenure on repetitions and decreases it when repetition evidence disappears."
        },
        new()
        {
            Id = TabuSearchComponentIds.BestSoFarAspiration,
            Name = "Best-so-far aspiration",
            Category = "aspiration",
            ImplementationType = "BestSoFarAspirationCriterion",
            Reference = "Glover (1989)",
            ScientificScope =
                "Releases a tabu move when its candidate objective strictly improves the global best."
        },
        new()
        {
            Id = TabuSearchComponentIds.EliteRestartIntensification,
            Name = "Elite restart intensification",
            Category = "intensification",
            ImplementationType = "ReactiveTabuSearchOptimizer (elite restart path)",
            Reference = "Glover (1989); Glover & Laguna (1997)",
            ScientificScope =
                "Optional restart from the best-owned solution after configured stagnation."
        },
        new()
        {
            Id = TabuSearchComponentIds.FrequencyPenaltyDiversification,
            Name = "Frequency-penalty diversification",
            Category = "diversification",
            ImplementationType = "ReactiveTabuSearchOptimizer (frequency-guided ranking)",
            Reference = "Glover (1989); Glover & Laguna (1997)",
            ScientificScope =
                "Optional linear selection penalty based on long-term candidate-attribute frequency."
        },
        new()
        {
            Id = TabuSearchComponentIds.ReactiveRandomWalkDiversification,
            Name = "Reactive random-walk escape",
            Category = "diversification",
            ImplementationType = "ReactiveTabuSearchOptimizer (escape path)",
            Reference = "Battiti & Tecchiolli (1994)",
            ScientificScope =
                "Uniformly sampled applicable moves; escape length follows moving-average cycle length."
        }
    ];

    private static readonly IReadOnlyDictionary<string, TabuSearchComponentDescriptor>
        ById =
        Entries.ToDictionary(
            static entry => entry.Id,
            StringComparer.Ordinal);

    public static IReadOnlyList<TabuSearchComponentDescriptor> All =>
        Entries;

    public static bool TryGet(
        string id,
        out TabuSearchComponentDescriptor? descriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return ById.TryGetValue(
            id,
            out descriptor);
    }

    public static TabuSearchComponentDescriptor GetRequired(
        string id)
    {
        if (!TryGet(
                id,
                out TabuSearchComponentDescriptor? descriptor))
        {
            throw new KeyNotFoundException(
                $"Unknown Tabu Search component id '{id}'.");
        }

        return descriptor!;
    }
}
