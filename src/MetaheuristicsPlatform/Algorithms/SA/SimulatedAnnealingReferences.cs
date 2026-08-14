using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.SA;

public static class SimulatedAnnealingReferences
{
    public static ScientificReference MetropolisEtAl1953 { get; } =
        new(
            "N. Metropolis, A. W. Rosenbluth, M. N. Rosenbluth, A. H. Teller, E. Teller",
            1953,
            "Equation of State Calculations by Fast Computing Machines",
            "Journal of Chemical Physics 21(6), 1087-1092",
            "10.1063/1.1699114");

    public static ScientificReference KirkpatrickGelattVecchi1983 { get; } =
        new(
            "S. Kirkpatrick, C. D. Gelatt Jr., M. P. Vecchi",
            1983,
            "Optimization by Simulated Annealing",
            "Science 220(4598), 671-680",
            "10.1126/science.220.4598.671");

    public static ScientificReference LundyMees1986 { get; } =
        new(
            "M. Lundy, A. Mees",
            1986,
            "Convergence of an annealing algorithm",
            "Mathematical Programming 34(1), 111-124",
            "10.1007/BF01582166");
}