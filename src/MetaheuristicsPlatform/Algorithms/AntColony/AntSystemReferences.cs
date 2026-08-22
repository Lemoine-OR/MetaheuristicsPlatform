using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Scientific references for the ACO foundation and reviewed descendants.</summary>
public static class AntSystemReferences
{
    public static ScientificReference DorigoManiezzoColorni1996 { get; } =
        new(
            "Dorigo, Maniezzo & Colorni",
            1996,
            "Ant System: Optimization by a Colony of Cooperating Agents",
            "IEEE Transactions on Systems, Man, and Cybernetics, Part B 26(1), 29-41",
            "10.1109/3477.484436");

    public static ScientificReference DorigoGambardella1997 { get; } =
        new(
            "Dorigo & Gambardella",
            1997,
            "Ant Colony System: A Cooperative Learning Approach to the Traveling Salesman Problem",
            "IEEE Transactions on Evolutionary Computation 1(1), 53-66",
            "10.1109/4235.585892");

    public static ScientificReference StutzleHoos2000 { get; } =
        new(
            "Stutzle & Hoos",
            2000,
            "MAX-MIN Ant System",
            "Future Generation Computer Systems 16(8), 889-914",
            "10.1016/S0167-739X(00)00043-1");
}
