using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>Scientific references for Demon-based acceptance methods.</summary>
public static class DemonAcceptanceReferences
{
    public static ScientificReference Creutz1983 { get; } = new(
        "Michael Creutz",
        1983,
        "Microcanonical Monte Carlo Simulation",
        "Physical Review Letters 50(19), 1411-1414",
        "10.1103/PhysRevLett.50.1411");

    public static ScientificReference Talbi2009 { get; } = new(
        "El-Ghazali Talbi",
        2009,
        "Single-Solution Based Metaheuristics",
        "Metaheuristics: From Design to Implementation, Chapter 2",
        "10.1002/9780470496916.ch2");

    public static ScientificReference WoodDowns1998 { get; } = new(
        "Ian A. Wood; Tom Downs",
        1998,
        "Demon algorithms and their application to optimization problems",
        "IEEE World Congress on Computational Intelligence / IJCNN, 1661-1666");

    public static ScientificReference ZimmermannSalamon1992 { get; } = new(
        "Theo Zimmermann; Peter Salamon",
        1992,
        "The demon algorithm",
        "International Journal of Computer Mathematics 42(1-2), 21-31",
        "10.1080/00207169208804047");
}
