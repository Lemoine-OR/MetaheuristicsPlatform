using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Canonical and reviewed CMA-ES references.</summary>
public static class CmaEsReferences
{
    public static ScientificReference HansenOstermeier2001 { get; } =
        new(
            "Hansen & Ostermeier",
            2001,
            "Completely Derandomized Self-Adaptation in Evolution Strategies",
            "Evolutionary Computation 9(2), 159-195",
            "10.1162/106365601750190398");

    public static ScientificReference HansenMullerKoumoutsakos2003 { get; } =
        new(
            "Hansen, Muller & Koumoutsakos",
            2003,
            "Reducing the Time Complexity of the Derandomized Evolution Strategy with Covariance Matrix Adaptation (CMA-ES)",
            "Evolutionary Computation 11(1), 1-18",
            "10.1162/106365603321828970");

    public static ScientificReference AugerHansen2012 { get; } =
        new(
            "Auger & Hansen",
            2012,
            "Tutorial CMA-ES: Evolution Strategies and Covariance Matrix Adaptation",
            "GECCO 2012 Companion, 827-847",
            "10.1145/2330784.2330919");
    public static ScientificReference RosHansen2008 { get; } =
        new(
            "Ros & Hansen",
            2008,
            "A Simple Modification in CMA-ES Achieving Linear Time and Space Complexity",
            "PPSN X, LNCS 5199, 296-305",
            "10.1007/978-3-540-87700-4_30");

    public static ScientificReference JastrebskiArnold2006 { get; } =
        new(
            "Jastrebski & Arnold",
            2006,
            "Improving Evolution Strategies through Active Covariance Matrix Adaptation",
            "IEEE Congress on Evolutionary Computation",
            "10.1109/CEC.2006.1688662");

    public static ScientificReference HansenRos2010 { get; } =
        new(
            "Hansen & Ros",
            2010,
            "Benchmarking a Weighted Negative Covariance Matrix Update on the BBOB-2010 Noiseless Testbed",
            "GECCO 2010 Companion, 1673-1680",
            "10.1145/1830761.1830788");

    public static ScientificReference AugerHansen2005 { get; } =
        new(
            "Auger & Hansen",
            2005,
            "A Restart CMA Evolution Strategy with Increasing Population Size",
            "IEEE CEC 2005, volume 2, 1769-1776",
            "10.1109/CEC.2005.1554902");

    public static ScientificReference Hansen2009Bipop { get; } =
        new(
            "Hansen",
            2009,
            "Benchmarking a BI-Population CMA-ES on the BBOB-2009 Function Testbed",
            "GECCO 2009 Companion, 2389-2396",
            "10.1145/1570256.1570333");

}
