using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.ArtificialBeeColony;

/// <summary>Canonical scientific references for Artificial Bee Colony.</summary>
public static class ArtificialBeeColonyReferences
{
    public static ScientificReference KarabogaBasturk2007 { get; } =
        new(
            "Karaboga & Basturk",
            2007,
            "A Powerful and Efficient Algorithm for Numerical Function Optimization: Artificial Bee Colony (ABC) Algorithm",
            "Journal of Global Optimization 39(3), 459-471",
            "10.1007/s10898-007-9149-x");

    public static ScientificReference KarabogaBasturk2008 { get; } =
        new(
            "Karaboga & Basturk",
            2008,
            "On the Performance of Artificial Bee Colony (ABC) Algorithm",
            "Applied Soft Computing 8(1), 687-697",
            "10.1016/j.asoc.2007.05.007");
}
