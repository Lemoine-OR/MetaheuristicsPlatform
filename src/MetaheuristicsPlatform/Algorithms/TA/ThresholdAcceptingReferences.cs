using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>Scientific references for Threshold Accepting.</summary>
public static class ThresholdAcceptingReferences
{
    /// <summary>Canonical Threshold Accepting paper.</summary>
    public static ScientificReference DueckScheuer1990 { get; } = new(
        "Gunter Dueck; Tobias Scheuer",
        1990,
        "Threshold accepting: A general purpose optimization algorithm appearing superior to simulated annealing",
        "Journal of Computational Physics 90(1), 161-175",
        "10.1016/0021-9991(90)90201-B");

    /// <summary>Peer-reviewed implementation study of Threshold Accepting.</summary>
    public static ScientificReference WinkerFang1997 { get; } = new(
        "Peter Winker; Kai-Tai Fang",
        1997,
        "Application of Threshold-Accepting to the Evaluation of the Discrepancy of a Set of Points",
        "SIAM Journal on Numerical Analysis 34(5), 2028-2042",
        "10.1137/S0036142995286076");

    /// <summary>Non-monotone self-tuning threshold acceptance reviewed for later work.</summary>
    public static ScientificReference HuKahngTsao1995 { get; } = new(
        "T. C. Hu; Andrew B. Kahng; Chung-Wen Albert Tsao",
        1995,
        "Old Bachelor Acceptance: A New Class of Non-Monotone Threshold Accepting Methods",
        "ORSA Journal on Computing 7(4), 417-425",
        "10.1287/ijoc.7.4.417");
}