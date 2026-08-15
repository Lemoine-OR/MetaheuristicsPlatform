using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Scientific references for Local Search, restart methods and iterated local search.</summary>
public static class NeighborhoodSearchReferences
{
    /// <summary>Talbi's unified metaheuristics design and implementation reference.</summary>
    public static ScientificReference Talbi2009 { get; } = new(
        "El-Ghazali Talbi",
        2009,
        "Metaheuristics: From Design to Implementation",
        "Wiley",
        "10.1002/9780470496916");

    /// <summary>Canonical Handbook of Metaheuristics chapter on multi-start methods.</summary>
    public static ScientificReference Marti2003 { get; } = new(
        "Rafael Martí",
        2003,
        "Multi-Start Methods",
        "Handbook of Metaheuristics, pp. 355-368",
        "10.1007/0-306-48056-5_12");

    /// <summary>Canonical Handbook of Metaheuristics chapter defining Iterated Local Search.</summary>
    public static ScientificReference LourencoMartinStutzle2003 { get; } = new(
        "Helena R. Lourenço; Olivier C. Martin; Thomas Stützle",
        2003,
        "Iterated Local Search",
        "Handbook of Metaheuristics, pp. 320-353",
        "10.1007/0-306-48056-5_11");
}
