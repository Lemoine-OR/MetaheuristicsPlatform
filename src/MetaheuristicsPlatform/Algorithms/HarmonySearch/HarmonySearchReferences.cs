using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Scientific provenance for the canonical Harmony Search foundation and explicitly
/// separated later variants.
/// </summary>
public static class HarmonySearchReferences
{
    public static ScientificReference GeemKimLoganathan2001 { get; } =
        new(
            "Z. W. Geem, J. H. Kim, G. V. Loganathan",
            2001,
            "A New Heuristic Optimization Algorithm: Harmony Search",
            "SIMULATION 76(2), 60-68",
            "10.1177/003754970107600201");

    public static ScientificReference MahdaviFesangharyDamangir2007 { get; } =
        new(
            "M. Mahdavi, M. Fesanghary, E. Damangir",
            2007,
            "An improved harmony search algorithm for solving optimization problems",
            "Applied Mathematics and Computation 188(2), 1567-1579",
            "10.1016/j.amc.2006.11.033");

    public static ScientificReference OmranMahdavi2008 { get; } =
        new(
            "M. G. H. Omran, M. Mahdavi",
            2008,
            "Global-best harmony search",
            "Applied Mathematics and Computation 198(2), 643-656",
            "10.1016/j.amc.2007.09.004");
}