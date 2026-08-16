using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Canonical scientific references for Variable Neighborhood Search.</summary>
public static class VariableNeighborhoodSearchReferences
{
    /// <summary>Original Variable Neighborhood Search paper.</summary>
    public static ScientificReference MladenovicHansen1997 { get; } = new(
        "Nenad Mladenovic; Pierre Hansen",
        1997,
        "Variable neighborhood search",
        "Computers & Operations Research 24(11), 1097-1100",
        "10.1016/S0305-0548(97)00031-2");

    /// <summary>Principles, basic schemes and applications of VNS.</summary>
    public static ScientificReference HansenMladenovic2001 { get; } = new(
        "Pierre Hansen; Nenad Mladenovic",
        2001,
        "Variable neighborhood search: Principles and applications",
        "European Journal of Operational Research 130(3), 449-467",
        "10.1016/S0377-2217(00)00100-4");
}
