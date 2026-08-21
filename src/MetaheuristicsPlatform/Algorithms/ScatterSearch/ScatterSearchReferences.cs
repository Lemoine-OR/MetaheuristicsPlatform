using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>Scientific references for Scatter Search.</summary>
public static class ScatterSearchReferences
{
    public static ScientificReference MartiLagunaGlover2006 { get; } = new(
        "Rafael Martí; Manuel Laguna; Fred Glover",
        2006,
        "Principles of scatter search",
        "European Journal of Operational Research 169(2), 359-372",
        "10.1016/j.ejor.2004.08.004");

    public static ScientificReference LagunaMarti2003 { get; } = new(
        "Manuel Laguna; Rafael Martí",
        2003,
        "Scatter Search: Methodology and Implementations in C",
        "Operations Research/Computer Science Interfaces Series 24, Springer",
        "10.1007/978-1-4615-0337-8");

    public static ScientificReference GloverLagunaMarti2004 { get; } = new(
        "Fred Glover; Manuel Laguna; Rafael Martí",
        2004,
        "Scatter Search and Path Relinking: Foundations and Advanced Designs",
        "New Optimization Techniques in Engineering, 87-99",
        "10.1007/978-3-540-39930-8_4");
}
