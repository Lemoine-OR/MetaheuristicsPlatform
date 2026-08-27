using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.PSO.Speciation;

public static class SpeciesBasedPsoReferences
{
    public static ScientificReference ParrottLi2006 { get; } =
        new(
            "Parrott & Li",
            2006,
            "Locating and tracking multiple dynamic optima by a particle swarm model using speciation",
            "IEEE Transactions on Evolutionary Computation 10(4), 440-458",
            "10.1109/TEVC.2005.859468");
}
