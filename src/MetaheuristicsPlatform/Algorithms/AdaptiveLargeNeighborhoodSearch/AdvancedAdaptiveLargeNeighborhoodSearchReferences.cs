using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public static class AdvancedAdaptiveLargeNeighborhoodSearchReferences
{
    public static ScientificReference SarasolaEtAl2020 { get; } =
        new(
            "Sarasola et al.",
            2020,
            "Adaptive large neighborhood search for the vehicle routing problem with synchronization constraints at the delivery location",
            "Networks 76(3), 355-376",
            "10.1002/net.21905");

    public static ScientificReference Hendel2022 { get; } =
        new(
            "Hendel",
            2022,
            "Adaptive large neighborhood search for mixed integer programming",
            "Mathematical Programming Computation 14, 185-221",
            "10.1007/s12532-021-00209-7");

    public static ScientificReference SantiniRopkeHvattum2018 { get; } =
        new(
            "Santini, Ropke & Hvattum",
            2018,
            "A comparison of acceptance criteria for the adaptive large neighbourhood search metaheuristic",
            "Journal of Heuristics 24(5), 783-815",
            "10.1007/s10732-018-9377-x");
}
