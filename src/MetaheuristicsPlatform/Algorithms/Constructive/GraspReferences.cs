using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Canonical scientific references for GRASP.</summary>
public static class GraspReferences
{
    /// <summary>Early probabilistic constructive predecessor of GRASP.</summary>
    public static ScientificReference FeoResende1989 { get; } = new(
        "Thomas A. Feo; Mauricio G. C. Resende",
        1989,
        "A probabilistic heuristic for a computationally difficult set covering problem",
        "Operations Research Letters 8(2), 67-71",
        "10.1016/0167-6377(89)90002-3");

    /// <summary>Canonical GRASP methodology paper.</summary>
    public static ScientificReference FeoResende1995 { get; } = new(
        "Thomas A. Feo; Mauricio G. C. Resende",
        1995,
        "Greedy Randomized Adaptive Search Procedures",
        "Journal of Global Optimization 6(2), 109-133",
        "10.1007/BF01096763");

    /// <summary>Reactive GRASP probability adaptation.</summary>
    public static ScientificReference PraisRibeiro2000 { get; } = new(
        "Marcelo Prais; Celso C. Ribeiro",
        2000,
        "Reactive GRASP: An Application to a Matrix Decomposition Problem in TDMA Traffic Assignment",
        "INFORMS Journal on Computing 12(3), 164-176",
        "10.1287/ijoc.12.3.164.12639");

    /// <summary>GRASP + path-relinking framework and implementation template.</summary>
    public static ScientificReference ResendeRibeiro2003 { get; } = new(
        "Mauricio G. C. Resende; Celso C. Ribeiro",
        2003,
        "GRASP and path-relinking: Recent advances and applications",
        "AT&T Labs Research Technical Report / MIC 2003 tutorial");

    /// <summary>Peer-reviewed GRASP with path relinking implementation and variants.</summary>
    public static ScientificReference AiexResendePardalosToraldo2005 { get; } = new(
        "Renata M. Aiex; Mauricio G. C. Resende; Panos M. Pardalos; Gerardo Toraldo",
        2005,
        "GRASP with Path Relinking for Three-Index Assignment",
        "INFORMS Journal on Computing 17(2), 224-247",
        "10.1287/ijoc.1030.0059");
    /// <summary>Canonical review of advanced path-relinking implementation strategies.</summary>
    public static ScientificReference RibeiroResende2012 { get; } = new(
        "Celso C. Ribeiro; Mauricio G. C. Resende",
        2012,
        "Path-relinking intensification methods for stochastic local search algorithms",
        "Journal of Heuristics 18(2), 193-214",
        "10.1007/s10732-011-9167-1");
}
