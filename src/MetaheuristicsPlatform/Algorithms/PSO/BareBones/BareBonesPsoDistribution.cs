namespace MetaheuristicsPlatform.Algorithms.PSO.BareBones;

public readonly record struct BareBonesPsoDistribution(
    double Mean,
    double StandardDeviation)
{
    public static BareBonesPsoDistribution From(
        double personalBest,
        double neighborhoodBest) =>
        new(
            0.5 * (personalBest + neighborhoodBest),
            Math.Abs(personalBest - neighborhoodBest));
}
