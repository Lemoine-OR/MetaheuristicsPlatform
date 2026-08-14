using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.DE;

public static class DifferentialEvolutionReferences
{
    public static ScientificReference StornPrice1997 { get; } =
        new(
            Authors: "Rainer Storn; Kenneth Price",
            Year: 1997,
            Title:
                "Differential Evolution — A Simple and Efficient Heuristic " +
                "for Global Optimization over Continuous Spaces",
            Venue:
                "Journal of Global Optimization, 11(4), 341–359",
            Doi:
                "10.1023/A:1008202821328");
}