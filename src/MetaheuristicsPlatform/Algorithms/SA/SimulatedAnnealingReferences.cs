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

    public static ScientificReference GemanGeman1984 { get; } =
        new(
            "S. Geman, D. Geman",
            1984,
            "Stochastic Relaxation, Gibbs Distributions, and the Bayesian Restoration of Images",
            "IEEE Transactions on Pattern Analysis and Machine Intelligence 6(6), 721-741",
            "10.1109/TPAMI.1984.4767596");

    public static ScientificReference AartsVanLaarhoven1985 { get; } =
        new(
            "E. H. L. Aarts, P. J. M. van Laarhoven",
            1985,
            "Statistical Cooling: A General Approach to Combinatorial Optimization Problems",
            "Philips Journal of Research 40(4), 193-226");

    public static ScientificReference LundyMees1986 { get; } =
        new(
            "M. Lundy, A. Mees",
            1986,
            "Convergence of an annealing algorithm",
            "Mathematical Programming 34(1), 111-124",
            "10.1007/BF01582166");

    public static ScientificReference HuangRomeoSangiovanniVincentelli1986 { get; } =
        new(
            "M. D. Huang, F. Romeo, A. Sangiovanni-Vincentelli",
            1986,
            "An Efficient General Cooling Schedule for Simulated Annealing",
            "Proceedings of IEEE ICCAD 1986, 381-384");

    public static ScientificReference SzuHartley1987 { get; } =
        new(
            "H. Szu, R. Hartley",
            1987,
            "Fast Simulated Annealing",
            "Physics Letters A 122(3-4), 157-162",
            "10.1016/0375-9601(87)90796-1");

    public static ScientificReference Hajek1988 { get; } =
        new(
            "B. Hajek",
            1988,
            "Cooling Schedules for Optimal Annealing",
            "Mathematics of Operations Research 13(2), 311-329",
            "10.1287/moor.13.2.311");

    public static ScientificReference LamDelosme1988 { get; } =
        new(
            "J. Lam, J.-M. Delosme",
            1988,
            "Performance of a New Annealing Schedule",
            "Proceedings of the 25th ACM/IEEE Design Automation Conference, 306-311",
            "10.1109/DAC.1988.14775");

    public static ScientificReference SalamonEtAl1988 { get; } =
        new(
            "P. Salamon, J. D. Nulton, J. R. Harland, J. Pedersen, G. Ruppeiner, L. Liao",
            1988,
            "Simulated Annealing with Constant Thermodynamic Speed",
            "Computer Physics Communications 49(3), 423-428",
            "10.1016/0010-4655(88)90003-3");

    public static ScientificReference OttenVanGinneken1989 { get; } =
        new(
            "R. H. J. M. Otten, L. P. P. P. van Ginneken",
            1989,
            "The Annealing Algorithm",
            "Kluwer Academic Publishers",
            "10.1007/978-1-4613-1627-5");

    public static ScientificReference StrenskiKirkpatrick1991 { get; } =
        new(
            "P. N. Strenski, S. Kirkpatrick",
            1991,
            "Analysis of finite length annealing schedules",
            "Algorithmica 6(3), 346-366");

    public static ScientificReference Ingber1989 { get; } =
        new(
            "L. Ingber",
            1989,
            "Very Fast Simulated Re-Annealing",
            "Mathematical and Computer Modelling 12(8), 967-973",
            "10.1016/0895-7177(89)90202-1");

    public static ScientificReference TsallisStariolo1996 { get; } =
        new(
            "C. Tsallis, D. A. Stariolo",
            1996,
            "Generalized Simulated Annealing",
            "Physica A 233(1-2), 395-406",
            "10.1016/S0378-4371(96)00271-3");

    public static ScientificReference CohnFielding1999 { get; } =
        new(
            "H. Cohn, M. Fielding",
            1999,
            "Simulated Annealing: Searching for an Optimal Temperature Schedule",
            "SIAM Journal on Optimization 9(3), 779-802",
            "10.1137/S1052623497329683");

    public static ScientificReference TrikiColletteSiarry2005 { get; } =
        new(
            "E. Triki, Y. Collette, P. Siarry",
            2005,
            "A theoretical study on the behavior of simulated annealing leading to a new cooling schedule",
            "European Journal of Operational Research 166(1), 77-92",
            "10.1016/j.ejor.2004.03.035");
}
