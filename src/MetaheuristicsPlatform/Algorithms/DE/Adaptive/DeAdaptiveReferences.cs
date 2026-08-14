using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public static class DeAdaptiveReferences
{
    public static ScientificReference BrestEtAl2006 { get; } =
        new(
            Authors:
                "Janez Brest; Saso Greiner; Borko Boskovic; Marjan Mernik; Viljem Zumer",
            Year: 2006,
            Title:
                "Self-Adapting Control Parameters in Differential Evolution: " +
                "A Comparative Study on Numerical Benchmark Problems",
            Venue:
                "IEEE Transactions on Evolutionary Computation, 10(6), 646-657",
            Doi:
                "10.1109/TEVC.2006.872133");

    public static ScientificReference ZhangSanderson2009 { get; } =
        new(
            Authors:
                "Jingqiao Zhang; Arthur C. Sanderson",
            Year: 2009,
            Title:
                "JADE: Adaptive Differential Evolution With Optional External Archive",
            Venue:
                "IEEE Transactions on Evolutionary Computation, 13(5), 945-958",
            Doi:
                "10.1109/TEVC.2009.2014613");

    public static ScientificReference TanabeFukunaga2013 { get; } =
        new(
            Authors:
                "Ryoji Tanabe; Alex Fukunaga",
            Year: 2013,
            Title:
                "Success-History Based Parameter Adaptation for Differential Evolution",
            Venue:
                "2013 IEEE Congress on Evolutionary Computation, 71-78",
            Doi:
                "10.1109/CEC.2013.6557555");

    public static ScientificReference TanabeFukunaga2014 { get; } =
        new(
            Authors:
                "Ryoji Tanabe; Alex S. Fukunaga",
            Year: 2014,
            Title:
                "Improving the Search Performance of SHADE Using Linear Population Size Reduction",
            Venue:
                "2014 IEEE Congress on Evolutionary Computation, 1658-1665",
            Doi:
                "10.1109/CEC.2014.6900380");
}