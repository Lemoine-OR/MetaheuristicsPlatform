using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;

/// <summary>Scientific references for Large Neighborhood Search.</summary>
public static class LargeNeighborhoodSearchReferences
{
    public static ScientificReference Shaw1998 { get; } =
        new(
            "Shaw",
            1998,
            "Using Constraint Programming and Local Search Methods to Solve Vehicle Routing Problems",
            "Principles and Practice of Constraint Programming - CP98, LNCS 1520, 417-431",
            "10.1007/3-540-49481-2_30");

    public static ScientificReference PisingerRopke2010 { get; } =
        new(
            "Pisinger & Ropke",
            2010,
            "Large Neighborhood Search",
            "Handbook of Metaheuristics, 2nd ed., 399-419",
            "10.1007/978-1-4419-1665-5_13");
}
