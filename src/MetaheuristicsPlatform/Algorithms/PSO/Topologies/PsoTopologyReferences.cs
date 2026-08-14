using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Canonical scientific references used by the PSO topology catalog.
/// </summary>
public static class PsoTopologyReferences
{
    public static ScientificReference KennedyEberhart1995 { get; } = new(
        "James Kennedy; Russell Eberhart",
        1995,
        "Particle Swarm Optimization",
        "Proceedings of ICNN'95 - International Conference on Neural Networks",
        "10.1109/ICNN.1995.488968");

    public static ScientificReference Suganthan1999 { get; } = new(
        "P. N. Suganthan",
        1999,
        "Particle swarm optimiser with neighbourhood operator",
        "Congress on Evolutionary Computation (CEC 1999)",
        "10.1109/CEC.1999.785514");

    public static ScientificReference KennedyMendes2002 { get; } = new(
        "James Kennedy; Rui Mendes",
        2002,
        "Population Structure and Particle Swarm Performance",
        "Congress on Evolutionary Computation (CEC 2002)",
        "10.1109/CEC.2002.1004493");

    public static ScientificReference JansonMiddendorf2003 { get; } = new(
        "Stefan Janson; Martin Middendorf",
        2003,
        "A hierarchical particle swarm optimizer",
        "Congress on Evolutionary Computation (CEC 2003)",
        "10.1109/CEC.2003.1299745");

    public static ScientificReference MendesKennedyNeves2004 { get; } = new(
        "Rui Mendes; James Kennedy; José Neves",
        2004,
        "The Fully Informed Particle Swarm: Simpler, Maybe Better",
        "IEEE Transactions on Evolutionary Computation 8(3), 204-210",
        "10.1109/TEVC.2004.826074");

    public static ScientificReference ZhangYi2011 { get; } = new(
        "Chenggong Zhang; Zhang Yi",
        2011,
        "Scale-free fully informed particle swarm optimization algorithm",
        "Information Sciences 181(20), 4550-4568",
        "10.1016/j.ins.2011.02.026");

    public static ScientificReference GongZhang2013 { get; } = new(
        "Yue-Jiao Gong; Jun Zhang",
        2013,
        "Small-world particle swarm optimization with topology adaptation",
        "Genetic and Evolutionary Computation Conference (GECCO 2013)",
        "10.1145/2463372.2463381");

    public static ScientificReference ElDorEtAl2015 { get; } = new(
        "Abbas El Dor; David Lemoine; Maurice Clerc; Patrick Siarry; Laurent Deroussi; Michel Gourgand",
        2015,
        "Dynamic cluster in particle swarm optimization algorithm",
        "Natural Computing 14(4), 655-672",
        "10.1007/s11047-014-9465-2");
}