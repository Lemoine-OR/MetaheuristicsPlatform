using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Scientific references for advanced Variable Neighborhood Search variants.</summary>
public static class AdvancedVariableNeighborhoodSearchReferences
{
    /// <summary>Canonical tutorial covering RVNS, GVNS, SVNS and advanced VNS variants.</summary>
    public static ScientificReference HansenMladenovicTodosijevicHanafi2017 { get; } = new(
        "Pierre Hansen; Nenad Mladenovic; Raca Todosijevic; Said Hanafi",
        2017,
        "Variable neighborhood search: basics and variants",
        "EURO Journal on Computational Optimization 5(3), 423-454",
        "10.1007/s13675-016-0075-x");
}
