using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.CrossEntropy;

/// <summary>Scientific references for the Cross-Entropy Method.</summary>
public static class CrossEntropyReferences
{
    public static ScientificReference Rubinstein1999 { get; } =
        new(
            "Rubinstein",
            1999,
            "The Cross-Entropy Method for Combinatorial and Continuous Optimization",
            "Methodology and Computing in Applied Probability 1(2), 127-190",
            "10.1023/A:1010091220143");

    public static ScientificReference DeBoerKroeseMannorRubinstein2005 { get; } =
        new(
            "de Boer, Kroese, Mannor & Rubinstein",
            2005,
            "A Tutorial on the Cross-Entropy Method",
            "Annals of Operations Research 134(1), 19-67",
            "10.1007/s10479-005-5724-z");

    public static ScientificReference KroesePorotskyRubinstein2006 { get; } =
        new(
            "Kroese, Porotsky & Rubinstein",
            2006,
            "The Cross-Entropy Method for Continuous Multi-Extremal Optimization",
            "Methodology and Computing in Applied Probability 8(3), 383-407",
            "10.1007/s11009-006-9753-0");
}
