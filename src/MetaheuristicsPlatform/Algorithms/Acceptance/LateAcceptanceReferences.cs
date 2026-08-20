using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>
/// Scientific references for Late Acceptance Hill Climbing and reviewed Demon methods.
/// </summary>
public static class LateAcceptanceReferences
{
    public static ScientificReference BurkeBykov2017 { get; } = new(
        "Edmund K. Burke; Yuri Bykov",
        2017,
        "The late acceptance Hill-Climbing heuristic",
        "European Journal of Operational Research 258(1), 70-78",
        "10.1016/j.ejor.2016.07.012");

    public static ScientificReference ZimmermannSalamon1992 { get; } = new(
        "Theo Zimmermann; Peter Salamon",
        1992,
        "The demon algorithm",
        "International Journal of Computer Mathematics 42(1-2), 21-31",
        "10.1080/00207169208804047");
}
