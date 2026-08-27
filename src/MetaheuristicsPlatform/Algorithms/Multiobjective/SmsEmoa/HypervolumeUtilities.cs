using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.SmsEmoa;
internal static class HypervolumeUtilities
{
    public static double Contribution(
        IReadOnlyList<MoCandidate> front,
        int index,
        IReadOnlyList<OptimizationSense> senses)
    {
        double total = Hypervolume(front, senses);
        List<MoCandidate> reduced = new(front.Count - 1);
        for (int i = 0; i < front.Count; i++)
            if (i != index) reduced.Add(front[i]);
        return Math.Max(0.0, total - Hypervolume(reduced, senses));
    }
    private static double Hypervolume(
        IReadOnlyList<MoCandidate> points,
        IReadOnlyList<OptimizationSense> senses)
    {
        if (points.Count == 0) return 0.0;
        int dimensions = senses.Count;
        double[][] normalized = new double[points.Count][];
        double[] reference = new double[dimensions];
        for (int i = 0; i < points.Count; i++)
        {
            normalized[i] = new double[dimensions];
            for (int objective = 0; objective < dimensions; objective++)
                normalized[i][objective] =
                    MultiobjectiveToolkit.Normalize(points[i].Objectives[objective], senses[objective]);
        }
        for (int objective = 0; objective < dimensions; objective++)
        {
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            for (int i = 0; i < normalized.Length; i++)
            {
                min = Math.Min(min, normalized[i][objective]);
                max = Math.Max(max, normalized[i][objective]);
            }
            reference[objective] = max + Math.Max(1e-9, 0.1 * Math.Max(max - min, 1e-9));
        }
        return Recursive(normalized, reference, dimensions);
    }
    private static double Recursive(double[][] points, double[] reference, int dimensions)
    {
        if (points.Length == 0) return 0.0;
        if (dimensions == 1)
        {
            double best = points.Min(point => point[0]);
            return Math.Max(0.0, reference[0] - best);
        }
        double volume = 0.0;
        double bound = reference[dimensions - 1];
        double[][] ordered = points.OrderBy(point => point[dimensions - 1]).ToArray();
        for (int i = ordered.Length - 1; i >= 0; i--)
        {
            double value = ordered[i][dimensions - 1];
            double height = bound - value;
            if (height > 0.0)
            {
                double[][] slice = new double[i + 1][];
                for (int row = 0; row <= i; row++)
                {
                    slice[row] = new double[dimensions - 1];
                    Array.Copy(ordered[row], slice[row], dimensions - 1);
                }
                double[] subReference = new double[dimensions - 1];
                Array.Copy(reference, subReference, dimensions - 1);
                volume += Recursive(slice, subReference, dimensions - 1) * height;
                bound = value;
            }
        }
        return volume;
    }
}
