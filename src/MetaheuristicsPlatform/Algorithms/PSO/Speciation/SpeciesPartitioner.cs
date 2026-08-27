using System.Linq;
using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.PSO.Speciation;

public static class SpeciesPartitioner
{
    public static int[] AssignSpeciesSeeds(
        IReadOnlyList<double[]> personalBestPositions,
        ReadOnlySpan<double> personalBestFitness,
        OptimizationSense sense,
        double speciesRadius)
    {
        ArgumentNullException.ThrowIfNull(personalBestPositions);

        if (personalBestPositions.Count != personalBestFitness.Length ||
            personalBestPositions.Count == 0)
            throw new ArgumentException("Species state lengths must agree and be non-empty.");

        if (!double.IsFinite(speciesRadius) || speciesRadius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(speciesRadius));

        int count = personalBestPositions.Count;
        int[] order = Enumerable.Range(0, count).ToArray();
        double[] sortableFitness = personalBestFitness.ToArray();

        Array.Sort(
            order,
            (left, right) =>
            {
                double a = sortableFitness[left];
                double b = sortableFitness[right];

                if (sense.IsBetter(a, b)) return -1;
                if (sense.IsBetter(b, a)) return 1;
                return left.CompareTo(right);
            });

        int[] seedForParticle = Enumerable.Repeat(-1, count).ToArray();
        double squaredRadius = speciesRadius * speciesRadius;

        foreach (int candidateSeed in order)
        {
            if (seedForParticle[candidateSeed] >= 0)
                continue;

            seedForParticle[candidateSeed] = candidateSeed;
            double[] seedPosition = personalBestPositions[candidateSeed];

            foreach (int particle in order)
            {
                if (seedForParticle[particle] >= 0)
                    continue;

                if (SquaredDistance(seedPosition, personalBestPositions[particle]) <= squaredRadius)
                    seedForParticle[particle] = candidateSeed;
            }
        }

        return seedForParticle;
    }

    private static double SquaredDistance(double[] left, double[] right)
    {
        if (left.Length != right.Length)
            throw new ArgumentException("Species positions must have the same dimension.");

        double sum = 0.0;

        for (int d = 0; d < left.Length; d++)
        {
            double delta = left[d] - right[d];
            sum += delta * delta;
        }

        return sum;
    }
}
