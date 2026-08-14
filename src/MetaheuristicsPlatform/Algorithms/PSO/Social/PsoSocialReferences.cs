using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>Canonical scientific references for PSO influence models.</summary>
public static class PsoSocialReferences
{
    public static ScientificReference KennedyEberhart1995 { get; } = new(
        "James Kennedy; Russell Eberhart",
        1995,
        "Particle Swarm Optimization",
        "Proceedings of ICNN'95 - International Conference on Neural Networks",
        "10.1109/ICNN.1995.488968");

    public static ScientificReference ClercKennedy2002 { get; } = new(
        "Maurice Clerc; James Kennedy",
        2002,
        "The particle swarm - explosion, stability, and convergence in a multidimensional complex space",
        "IEEE Transactions on Evolutionary Computation 6(1), 58-73",
        "10.1109/4235.985692");

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
}