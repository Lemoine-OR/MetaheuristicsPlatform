namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>Stable component identifiers for memetic local-improvement and learning policies.</summary>
public static class MemeticAlgorithmComponentIds
{
    public const string EveryOffspring = "ma.local-search.every-offspring";
    public const string Periodic = "ma.local-search.periodic";
    public const string Probabilistic = "ma.local-search.probabilistic";
    public const string TopFraction = "ma.local-search.top-fraction";
    public const string AdaptiveStagnation = "ma.local-search.adaptive-stagnation";
    public const string LamarckianLearning = "ma.learning.lamarckian";
    public const string BaldwinianLearning = "ma.learning.baldwinian";
}
