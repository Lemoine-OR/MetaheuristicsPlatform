using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Canonical scientific references for GRASP.</summary>
public static class GraspReferences
{
    public static ScientificReference FeoResende1989 { get; } = new(
        "Thomas A. Feo; Mauricio G. C. Resende",
        1989,
        "A probabilistic heuristic for a computationally difficult set covering problem",
        "Operations Research Letters 8(2), 67-71",
        "10.1016/0167-6377(89)90002-3");

    public static ScientificReference FeoResende1995 { get; } = new(
        "Thomas A. Feo; Mauricio G. C. Resende",
        1995,
        "Greedy Randomized Adaptive Search Procedures",
        "Journal of Global Optimization 6(2), 109-133",
        "10.1007/BF01096763");

    public static ScientificReference PraisRibeiro2000 { get; } = new(
        "Marcelo Prais; Celso C. Ribeiro",
        2000,
        "Reactive GRASP: An Application to a Matrix Decomposition Problem in TDMA Traffic Assignment",
        "INFORMS Journal on Computing 12(3), 164-176",
        "10.1287/ijoc.12.3.164.12639");

    public static ScientificReference ResendeRibeiro2003 { get; } = new(
        "Mauricio G. C. Resende; Celso C. Ribeiro",
        2003,
        "GRASP and path-relinking: Recent advances and applications",
        "AT&T Labs Research Technical Report / MIC 2003 tutorial");

    /// <summary>Original peer-reviewed generational evolutionary path-relinking scheme.</summary>
    public static ScientificReference ResendeWerneck2004 { get; } = new(
        "Mauricio G. C. Resende; Renato F. Werneck",
        2004,
        "A Hybrid Heuristic for the p-Median Problem",
        "Journal of Heuristics 10(1), 59-88",
        "10.1023/B:HEUR.0000019986.96257.50");

    public static ScientificReference AiexResendePardalosToraldo2005 { get; } = new(
        "Renata M. Aiex; Mauricio G. C. Resende; Panos M. Pardalos; Gerardo Toraldo",
        2005,
        "GRASP with Path Relinking for Three-Index Assignment",
        "INFORMS Journal on Computing 17(2), 224-247",
        "10.1287/ijoc.1030.0059");

    /// <summary>Evolutionary GRASP/path-relinking implementation and comparative study.</summary>
    public static ScientificReference ResendeMartiGallegoDuarte2010 { get; } = new(
        "Mauricio G. C. Resende; Rafael Marti; Micael Gallego; Abraham Duarte",
        2010,
        "GRASP and path relinking for the max-min diversity problem",
        "Computers & Operations Research 37(3), 498-508",
        "10.1016/j.cor.2008.05.011");

    public static ScientificReference RibeiroResende2012 { get; } = new(
        "Celso C. Ribeiro; Mauricio G. C. Resende",
        2012,
        "Path-relinking intensification methods for stochastic local search algorithms",
        "Journal of Heuristics 18(2), 193-214",
        "10.1007/s10732-011-9167-1");
}