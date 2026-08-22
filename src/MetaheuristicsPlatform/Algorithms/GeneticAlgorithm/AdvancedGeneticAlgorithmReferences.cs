using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Scientific references for the advanced GA operator catalog.
/// Null DOI values are intentional where no DOI was verified for the original publication.
/// </summary>
public static class AdvancedGeneticAlgorithmReferences
{
    public static ScientificReference GoldbergDeb1991 { get; } = new(
        "David E. Goldberg; Kalyanmoy Deb",
        1991,
        "A Comparative Analysis of Selection Schemes Used in Genetic Algorithms",
        "Foundations of Genetic Algorithms 1, 69-93",
        "10.1016/B978-0-08-050684-5.50008-2");

    public static ScientificReference Syswerda1989 { get; } = new(
        "Gilbert Syswerda",
        1989,
        "Uniform Crossover in Genetic Algorithms",
        "Proceedings of the Third International Conference on Genetic Algorithms, 2-9",
        "10.5555/645512.657265");

    public static ScientificReference Syswerda1991 { get; } = new(
        "Gilbert Syswerda",
        1991,
        "A Study of Reproduction in Generational and Steady-State Genetic Algorithms",
        "Foundations of Genetic Algorithms 1, 94-101",
        "10.1016/B978-0-08-050684-5.50009-4");

    public static ScientificReference GoldbergLingle1985 { get; } = new(
        "David E. Goldberg; Robert Lingle",
        1985,
        "Alleles, Loci, and the Traveling Salesman Problem",
        "Proceedings of the First International Conference on Genetic Algorithms and Their Applications, 154-159",
        "10.5555/645511.657095");

    public static ScientificReference Davis1985 { get; } = new(
        "Lawrence Davis",
        1985,
        "Applying Adaptive Algorithms to Epistatic Domains",
        "Proceedings of the Ninth International Joint Conference on Artificial Intelligence, 162-164",
        "10.5555/1625135.1625164");

    public static ScientificReference DebAgrawal1995 { get; } = new(
        "Kalyanmoy Deb; Ram Bhushan Agrawal",
        1995,
        "Simulated Binary Crossover for Continuous Search Space",
        "Complex Systems 9(2), 115-148");

    public static ScientificReference DebPratapAgarwalMeyarivan2002 { get; } = new(
        "Kalyanmoy Deb; Amrit Pratap; Sameer Agarwal; T. Meyarivan",
        2002,
        "A fast and elitist multiobjective genetic algorithm: NSGA-II",
        "IEEE Transactions on Evolutionary Computation 6(2), 182-197",
        "10.1109/4235.996017");

    public static ScientificReference DebDeb2014 { get; } = new(
        "Kalyanmoy Deb; Debayan Deb",
        2014,
        "Analysing mutation schemes for real-parameter genetic algorithms",
        "International Journal of Artificial Intelligence and Soft Computing 4(1), 1-28",
        "10.1504/IJAISC.2014.059280");
}
