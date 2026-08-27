using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public static class ConstrictionPsoReferences
{
    public static ScientificReference ClercKennedy2002 { get; } =
        new(
            "Clerc & Kennedy",
            2002,
            "The particle swarm - explosion, stability, and convergence in a multidimensional complex space",
            "IEEE Transactions on Evolutionary Computation 6(1), 58-73",
            "10.1109/4235.985692");
}
