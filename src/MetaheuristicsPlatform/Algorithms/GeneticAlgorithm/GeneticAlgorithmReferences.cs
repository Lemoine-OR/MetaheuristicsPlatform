using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>Scientific references for the generic generational GA foundation.</summary>
public static class GeneticAlgorithmReferences
{
    public static ScientificReference EibenSmith2003 { get; } = new(
        "A. E. Eiben; J. E. Smith",
        2003,
        "Genetic Algorithms",
        "Introduction to Evolutionary Computing, Natural Computing Series, 37-69",
        "10.1007/978-3-662-05094-1_3");

    public static ScientificReference Whitley1994 { get; } = new(
        "Darrell Whitley",
        1994,
        "A genetic algorithm tutorial",
        "Statistics and Computing 4(2), 65-85",
        "10.1007/BF00175354");

    public static ScientificReference BlickleThiele1996 { get; } = new(
        "Tobias Blickle; Lothar Thiele",
        1996,
        "A Comparison of Selection Schemes used in Evolutionary Algorithms",
        "Evolutionary Computation 4(4), 361-394",
        "10.1162/EVCO.1996.4.4.361");
}
