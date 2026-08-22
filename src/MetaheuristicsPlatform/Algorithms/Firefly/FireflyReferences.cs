using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Firefly;

/// <summary>Scientific references for the Firefly Algorithm.</summary>
public static class FireflyReferences
{
    public static ScientificReference Yang2009 { get; } =
        new(
            "Yang",
            2009,
            "Firefly Algorithms for Multimodal Optimization",
            "Stochastic Algorithms: Foundations and Applications, LNCS 5792, 169-178",
            "10.1007/978-3-642-04944-6_14");

    public static ScientificReference Yang2010 { get; } =
        new(
            "Yang",
            2010,
            "Firefly Algorithm, Stochastic Test Functions and Design Optimisation",
            "International Journal of Bio-Inspired Computation 2(2), 78-84",
            "10.1504/IJBIC.2010.032124");
}
