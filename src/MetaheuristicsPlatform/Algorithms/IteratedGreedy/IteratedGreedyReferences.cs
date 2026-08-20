using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

/// <summary>Scientific references for canonical and reviewed Iterated Greedy methods.</summary>
public static class IteratedGreedyReferences
{
    public static ScientificReference RuizStutzle2007 { get; } = new(
        "Rubén Ruiz; Thomas Stützle",
        2007,
        "A simple and effective iterated greedy algorithm for the permutation flowshop scheduling problem",
        "European Journal of Operational Research 177(3), 2033-2049",
        "10.1016/j.ejor.2005.12.009");

    public static ScientificReference StutzleRuiz2025 { get; } = new(
        "Thomas Stützle; Rubén Ruiz",
        2025,
        "Iterated Greedy",
        "Handbook of Heuristics, 745-777",
        "10.1007/978-3-032-00385-0_10");

    // Reviewed for the advanced v0.38.0 line; not collapsed into flags in v0.37.0.
    public static ScientificReference RuizPanNaderi2019 { get; } = new(
        "Rubén Ruiz; Quan-Ke Pan; Bahman Naderi",
        2019,
        "Iterated Greedy methods for the distributed permutation flowshop scheduling problem",
        "Omega 83, 213-222",
        "10.1016/j.omega.2018.03.004");

    public static ScientificReference IteratedReferenceGreedy2017 { get; } = new(
        "Reviewed advanced Iterated Greedy lineage",
        2017,
        "Iterated reference greedy algorithm for solving distributed no-idle permutation flowshop scheduling problems",
        "Computers & Industrial Engineering 110, 413-423",
        "10.1016/j.cie.2017.06.025");

    public static ScientificReference AdaptiveDestruction2020 { get; } = new(
        "Reviewed adaptive Iterated Greedy lineage",
        2020,
        "An effective Iterated Greedy algorithm for the distributed permutation flowshop scheduling with due windows",
        "Applied Soft Computing 96, 106629",
        "10.1016/j.asoc.2020.106629");
}
