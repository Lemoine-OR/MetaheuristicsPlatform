using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>Scientific references for the v0.43 memetic-algorithm foundation.</summary>
public static class MemeticAlgorithmReferences
{
    public static ScientificReference Moscato1989 { get; } = new(
        "Pablo Moscato",
        1989,
        "On Evolution, Search, Optimization, Genetic Algorithms and Martial Arts: Towards Memetic Algorithms",
        "Caltech Concurrent Computation Program, Report 826");

    public static ScientificReference KrasnogorSmith2005 { get; } = new(
        "Natalio Krasnogor; James Smith",
        2005,
        "A Tutorial for Competent Memetic Algorithms: Model, Taxonomy, and Design Issues",
        "IEEE Transactions on Evolutionary Computation 9(5), 474-488",
        "10.1109/TEVC.2005.850260");
}
