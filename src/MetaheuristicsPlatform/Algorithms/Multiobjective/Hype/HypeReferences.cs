using MetaheuristicsPlatform.Classification;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Hype;
public static class HypeReferences
{
    public static ScientificReference BaderZitzler2011 { get; } =
        new(
            "Bader & Zitzler",
            2011,
            "HypE: An Algorithm for Fast Hypervolume-Based Many-Objective Optimization",
            "Evolutionary Computation 19(1), 45-76",
            "10.1162/EVCO_A_00009");
}
