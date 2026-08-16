using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Canonical scientific references for Guided Local Search.</summary>
public static class GuidedLocalSearchReferences
{
    /// <summary>Early peer-reviewed GLS/FLS paper on workforce scheduling.</summary>
    public static ScientificReference TsangVoudouris1997 { get; } = new(
        "Edward Tsang; Chris Voudouris",
        1997,
        "Fast local search and guided local search and their application to British Telecom's workforce scheduling problem",
        "Operations Research Letters 20(3), 119-127",
        "10.1016/S0167-6377(96)00042-9");

    /// <summary>Canonical detailed GLS paper and TSP study.</summary>
    public static ScientificReference VoudourisTsang1999 { get; } = new(
        "Christos Voudouris; Edward Tsang",
        1999,
        "Guided local search and its application to the traveling salesman problem",
        "European Journal of Operational Research 113(2), 469-499",
        "10.1016/S0377-2217(98)00099-X");
}
