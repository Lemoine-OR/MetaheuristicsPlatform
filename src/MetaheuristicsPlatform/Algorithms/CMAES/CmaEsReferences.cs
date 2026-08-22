using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Canonical and reviewed CMA-ES references.</summary>
public static class CmaEsReferences
{
    public static ScientificReference HansenOstermeier2001 { get; } =
        new(
            "Hansen & Ostermeier",
            2001,
            "Completely Derandomized Self-Adaptation in Evolution Strategies",
            "Evolutionary Computation 9(2), 159-195",
            "10.1162/106365601750190398");

    public static ScientificReference HansenMullerKoumoutsakos2003 { get; } =
        new(
            "Hansen, Muller & Koumoutsakos",
            2003,
            "Reducing the Time Complexity of the Derandomized Evolution Strategy with Covariance Matrix Adaptation (CMA-ES)",
            "Evolutionary Computation 11(1), 1-18",
            "10.1162/106365603321828970");

    public static ScientificReference AugerHansen2012 { get; } =
        new(
            "Auger & Hansen",
            2012,
            "Tutorial CMA-ES: Evolution Strategies and Covariance Matrix Adaptation",
            "GECCO 2012 Companion, 827-847",
            "10.1145/2330784.2330919");
}
