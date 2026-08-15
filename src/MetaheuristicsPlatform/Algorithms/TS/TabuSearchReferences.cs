using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>Primary scientific references for Tabu Search.</summary>
public static class TabuSearchReferences
{
    public static ScientificReference Glover1986 { get; } =
        new(
            "Fred Glover",
            1986,
            "Future paths for integer programming and links to artificial intelligence",
            "Computers & Operations Research 13(5), 533-549",
            "10.1016/0305-0548(86)90048-1");

    public static ScientificReference Glover1989 { get; } =
        new(
            "Fred Glover",
            1989,
            "Tabu Search-Part I",
            "ORSA Journal on Computing 1(3), 190-206",
            "10.1287/ijoc.1.3.190");

    public static ScientificReference Glover1990 { get; } =
        new(
            "Fred Glover",
            1990,
            "Tabu Search-Part II",
            "ORSA Journal on Computing 2(1), 4-32",
            "10.1287/ijoc.2.1.4");

    public static ScientificReference GloverLaguna1997 { get; } =
        new(
            "Fred Glover and Manuel Laguna",
            1997,
            "Tabu Search",
            "Kluwer/Springer",
            "10.1007/978-1-4615-6089-0");

    public static ScientificReference BattitiTecchiolli1994 { get; } =
        new(
            "Roberto Battiti and Giampietro Tecchiolli",
            1994,
            "The Reactive Tabu Search",
            "ORSA Journal on Computing 6(2), 126-140",
            "10.1287/ijoc.6.2.126");
}
