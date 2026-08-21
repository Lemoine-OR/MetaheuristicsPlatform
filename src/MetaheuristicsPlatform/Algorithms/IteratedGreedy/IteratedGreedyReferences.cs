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

    public static ScientificReference FernandezViagasFraminan2015 { get; } = new(
        "Victor Fernandez-Viagas; Jose M. Framinan",
        2015,
        "A bounded-search iterated greedy algorithm for the distributed permutation flowshop scheduling problem",
        "International Journal of Production Research 53(4), 1111-1123",
        "10.1080/00207543.2014.948578");

    public static ScientificReference DingEtAl2015 { get; } = new(
        "Jian-Ya Ding; Shiji Song; Jatinder N. D. Gupta; Rui Zhang; Raymond Chiong; Cheng Wu",
        2015,
        "An improved iterated greedy algorithm with a Tabu-based reconstruction strategy for the no-wait flowshop scheduling problem",
        "Applied Soft Computing 30, 604-613",
        "10.1016/j.asoc.2015.02.006");

    public static ScientificReference DuboisLacostePagnozziStutzle2017 { get; } = new(
        "Jérémie Dubois-Lacoste; Federico Pagnozzi; Thomas Stützle",
        2017,
        "An iterated greedy algorithm with optimization of partial solutions for the makespan permutation flowshop problem",
        "Computers & Operations Research 81, 160-166",
        "10.1016/j.cor.2016.12.021");

    public static ScientificReference IteratedReferenceGreedy2017 { get; } = new(
        "Kuo-Ching Ying; Shih-Wei Lin; Chen-Yang Cheng; Cheng-Ding He",
        2017,
        "Iterated reference greedy algorithm for solving distributed no-idle permutation flowshop scheduling problems",
        "Computers & Industrial Engineering 110, 413-423",
        "10.1016/j.cie.2017.06.025");

    public static ScientificReference RuizPanNaderi2019 { get; } = new(
        "Rubén Ruiz; Quan-Ke Pan; Bahman Naderi",
        2019,
        "Iterated Greedy methods for the distributed permutation flowshop scheduling problem",
        "Omega 83, 213-222",
        "10.1016/j.omega.2018.03.004");

    public static ScientificReference FernandezViagasFraminan2019 { get; } = new(
        "Victor Fernandez-Viagas; Jose M. Framinan",
        2019,
        "A best-of-breed iterated greedy for the permutation flowshop scheduling problem with makespan objective",
        "Computers & Operations Research 112, 104767",
        "10.1016/j.cor.2019.104767");

    public static ScientificReference JingPanGaoWang2020 { get; } = new(
        "Xue-Lei Jing; Quan-Ke Pan; Liang Gao; Yu-Long Wang",
        2020,
        "An effective Iterated Greedy algorithm for the distributed permutation flowshop scheduling with due windows",
        "Applied Soft Computing 96, 106629",
        "10.1016/j.asoc.2020.106629");

    public static ScientificReference LiPanLiGaoTasgetiren2021 { get; } = new(
        "Yuan-Zhen Li; Quan-Ke Pan; Jun-Qing Li; Liang Gao; Mehmet Fatih Tasgetiren",
        2021,
        "An Adaptive Iterated Greedy algorithm for distributed mixed no-idle permutation flowshop scheduling problems",
        "Swarm and Evolutionary Computation 63, 100874",
        "10.1016/j.swevo.2021.100874");

    public static ScientificReference ZhangQianHuLiYang2026 { get; } = new(
        "Sen Zhang; Bin Qian; Rong Hu; Kun Li; Jian-Bo Yang",
        2026,
        "A two-stage iterated greedy algorithm for distributed blocking flowshop scheduling problem",
        "Expert Systems with Applications 300, 130422",
        "10.1016/j.eswa.2025.130422");
}
