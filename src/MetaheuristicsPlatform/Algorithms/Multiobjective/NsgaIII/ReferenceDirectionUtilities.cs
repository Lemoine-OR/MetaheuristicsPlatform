using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaIII;
internal static class ReferenceDirectionUtilities
{
    public static double[][] DasDennis(int objectives, int divisions)
    {
        if (objectives < 2) throw new ArgumentOutOfRangeException(nameof(objectives));
        if (divisions < 1) throw new ArgumentOutOfRangeException(nameof(divisions));
        List<double[]> result = new();
        int[] current = new int[objectives];
        Generate(result, current, 0, divisions, divisions);
        return result.ToArray();
    }
    public static double[][] NormalizeObjectives(
        IReadOnlyList<MoCandidate> candidates,
        IReadOnlyList<OptimizationSense> senses)
    {
        int n = candidates.Count;
        int m = senses.Count;
        double[] ideal = new double[m];
        for (int objective = 0; objective < m; objective++)
        {
            ideal[objective] = double.PositiveInfinity;
            for (int i = 0; i < n; i++)
                ideal[objective] = Math.Min(
                    ideal[objective],
                    MultiobjectiveToolkit.Normalize(candidates[i].Objectives[objective], senses[objective]));
        }
        double[][] translated = new double[n][];
        double[] maxima = new double[m];
        for (int i = 0; i < n; i++)
        {
            translated[i] = new double[m];
            for (int objective = 0; objective < m; objective++)
            {
                translated[i][objective] =
                    MultiobjectiveToolkit.Normalize(candidates[i].Objectives[objective], senses[objective]) -
                    ideal[objective];
                maxima[objective] = Math.Max(maxima[objective], translated[i][objective]);
            }
        }
        for (int i = 0; i < n; i++)
            for (int objective = 0; objective < m; objective++)
                translated[i][objective] /= Math.Max(maxima[objective], 1e-12);
        return translated;
    }
    public static (int Reference, double Distance) Associate(
        ReadOnlySpan<double> point,
        IReadOnlyList<double[]> directions)
    {
        int bestReference = 0;
        double bestDistance = double.PositiveInfinity;
        for (int reference = 0; reference < directions.Count; reference++)
        {
            double dot = 0.0;
            double norm = 0.0;
            for (int objective = 0; objective < point.Length; objective++)
            {
                dot += point[objective] * directions[reference][objective];
                norm += directions[reference][objective] * directions[reference][objective];
            }
            double scale = norm <= 0.0 ? 0.0 : dot / norm;
            double distance = 0.0;
            for (int objective = 0; objective < point.Length; objective++)
            {
                double delta = point[objective] - scale * directions[reference][objective];
                distance += delta * delta;
            }
            distance = Math.Sqrt(distance);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestReference = reference;
            }
        }
        return (bestReference, bestDistance);
    }
    private static void Generate(
        List<double[]> output,
        int[] current,
        int index,
        int remaining,
        int divisions)
    {
        if (index == current.Length - 1)
        {
            current[index] = remaining;
            double[] vector = new double[current.Length];
            for (int objective = 0; objective < current.Length; objective++)
                vector[objective] = current[objective] / (double)divisions;
            output.Add(vector);
            return;
        }
        for (int value = 0; value <= remaining; value++)
        {
            current[index] = value;
            Generate(output, current, index + 1, remaining - value, divisions);
        }
    }
}
