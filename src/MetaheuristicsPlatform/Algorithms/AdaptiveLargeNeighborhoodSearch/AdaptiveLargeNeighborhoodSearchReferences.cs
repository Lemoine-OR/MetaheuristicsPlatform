using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public static class AdaptiveLargeNeighborhoodSearchReferences
{
    public static ScientificReference RopkePisinger2006 { get; } =
        new(
            "Ropke & Pisinger",
            2006,
            "An Adaptive Large Neighborhood Search Heuristic for the Pickup and Delivery Problem with Time Windows",
            "Transportation Science 40(4), 455-472",
            "10.1287/trsc.1050.0135");

    public static ScientificReference PisingerRopke2007 { get; } =
        new(
            "Pisinger & Ropke",
            2007,
            "A General Heuristic for Vehicle Routing Problems",
            "Computers & Operations Research 34(8), 2403-2435",
            "10.1016/j.cor.2005.09.012");
}
