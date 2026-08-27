using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;

internal static class MultiobjectiveAdvancedToolkit
{
    public static double[] NormalizeObjectives(
        MoCandidate candidate,
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        int m = senses.Count;
        double[] result = new double[m];

        for (int objective = 0; objective < m; objective++)
        {
            double minimum = double.PositiveInfinity;
            double maximum = double.NegativeInfinity;

            for (int i = 0; i < population.Count; i++)
            {
                double value =
                    MultiobjectiveToolkit.Normalize(
                        population[i].Objectives[objective],
                        senses[objective]);

                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }

            double current =
                MultiobjectiveToolkit.Normalize(
                    candidate.Objectives[objective],
                    senses[objective]);

            result[objective] =
                (current - minimum) /
                Math.Max(maximum - minimum, 1e-12);
        }

        return result;
    }

    public static double ObjectiveDistance(
        MoCandidate first,
        MoCandidate second,
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        double[] a =
            NormalizeObjectives(
                first,
                population,
                senses);

        double[] b =
            NormalizeObjectives(
                second,
                population,
                senses);

        double sum = 0.0;

        for (int objective = 0; objective < a.Length; objective++)
        {
            double delta = a[objective] - b[objective];
            sum += delta * delta;
        }

        return Math.Sqrt(sum);
    }

    public static double DecisionDistance(
        ReadOnlySpan<double> first,
        ReadOnlySpan<double> second)
    {
        double sum = 0.0;

        for (int coordinate = 0; coordinate < first.Length; coordinate++)
        {
            double delta = first[coordinate] - second[coordinate];
            sum += delta * delta;
        }

        return Math.Sqrt(sum);
    }

    public static List<MoCandidate> Nondominated(
        IReadOnlyList<MoCandidate> candidates,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> result = new();

        for (int i = 0; i < candidates.Count; i++)
        {
            bool dominated = false;

            for (int j = 0; j < candidates.Count; j++)
            {
                if (i == j)
                    continue;

                if (ParetoDominance.Compare(
                        candidates[j].Objectives,
                        candidates[i].Objectives,
                        senses) < 0)
                {
                    dominated = true;
                    break;
                }
            }

            if (!dominated)
                result.Add(candidates[i]);
        }

        return result;
    }

    public static List<MoCandidate> TruncateByClustering(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<List<MoCandidate>> clusters =
            candidates
                .Select(
                    candidate =>
                        new List<MoCandidate>
                        {
                            MultiobjectiveToolkit.Clone(candidate)
                        })
                .ToList();

        while (clusters.Count > size)
        {
            int firstCluster = 0;
            int secondCluster = 1;
            double bestDistance = double.PositiveInfinity;

            for (int first = 0; first < clusters.Count; first++)
            {
                for (int second = first + 1;
                     second < clusters.Count;
                     second++)
                {
                    double distance =
                        AverageClusterDistance(
                            clusters[first],
                            clusters[second],
                            candidates,
                            senses);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        firstCluster = first;
                        secondCluster = second;
                    }
                }
            }

            clusters[firstCluster].AddRange(
                clusters[secondCluster]);

            clusters.RemoveAt(
                secondCluster);
        }

        List<MoCandidate> representatives =
            new(size);

        foreach (List<MoCandidate> cluster in clusters)
        {
            MoCandidate representative =
                cluster
                    .OrderBy(
                        candidate =>
                            cluster.Sum(
                                other =>
                                    MultiobjectiveAdvancedToolkit.ObjectiveDistance(
                                        candidate,
                                        other,
                                        candidates,
                                        senses)))
                    .First();

            representatives.Add(
                MultiobjectiveToolkit.Clone(
                    representative));
        }

        return representatives;
    }

    public static List<MoCandidate> TruncateByNearestNeighbor(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> working =
            candidates
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        while (working.Count > size)
        {
            int removeIndex = 0;
            double[]? bestSignature = null;

            for (int i = 0; i < working.Count; i++)
            {
                List<double> distances = new();

                for (int j = 0; j < working.Count; j++)
                {
                    if (i == j)
                        continue;

                    distances.Add(
                        ObjectiveDistance(
                            working[i],
                            working[j],
                            working,
                            senses));
                }

                distances.Sort();
                double[] signature = distances.ToArray();

                if (bestSignature is null ||
                    LexicographicallySmaller(
                        signature,
                        bestSignature))
                {
                    bestSignature = signature;
                    removeIndex = i;
                }
            }

            working.RemoveAt(removeIndex);
        }

        return working;
    }

    public static double VectorAngle(
        ReadOnlySpan<double> first,
        ReadOnlySpan<double> second)
    {
        double dot = 0.0;
        double firstNorm = 0.0;
        double secondNorm = 0.0;

        for (int i = 0; i < first.Length; i++)
        {
            dot += first[i] * second[i];
            firstNorm += first[i] * first[i];
            secondNorm += second[i] * second[i];
        }

        double denominator =
            Math.Sqrt(firstNorm) *
            Math.Sqrt(secondNorm);

        if (denominator <= 1e-15)
            return Math.PI / 2.0;

        double cosine =
            Math.Clamp(
                dot / denominator,
                -1.0,
                1.0);

        return Math.Acos(cosine);
    }

    public static double[] UnitDirection(
        MoCandidate candidate,
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        double[] vector =
            NormalizeObjectives(
                candidate,
                population,
                senses);

        double norm =
            Math.Sqrt(
                vector.Sum(
                    value =>
                        value * value));

        if (norm <= 1e-15)
            return vector;

        for (int i = 0; i < vector.Length; i++)
            vector[i] /= norm;

        return vector;
    }

    public static int TournamentByFitness(
        IReadOnlyList<MoCandidate> population,
        IRandomSource random,
        bool smallerIsBetter = true)
    {
        int first =
            random.NextInt32(
                population.Count);

        int second =
            random.NextInt32(
                population.Count);

        double a = population[first].Fitness;
        double b = population[second].Fitness;

        if (a == b)
            return random.NextDouble() < 0.5
                ? first
                : second;

        if (smallerIsBetter)
            return a < b ? first : second;

        return a > b ? first : second;
    }

    public static double[] RandomWeights(
        int objectives,
        IRandomSource random)
    {
        double[] weights =
            new double[objectives];

        double sum = 0.0;

        for (int objective = 0;
             objective < objectives;
             objective++)
        {
            double value =
                -Math.Log(
                    Math.Max(
                        random.NextDouble(),
                        1e-15));

            weights[objective] = value;
            sum += value;
        }

        for (int objective = 0;
             objective < objectives;
             objective++)
            weights[objective] /= sum;

        return weights;
    }

    public static double Pbi(
        ReadOnlySpan<double> normalizedObjectives,
        ReadOnlySpan<double> direction,
        double theta)
    {
        double norm = 0.0;

        for (int i = 0; i < direction.Length; i++)
            norm += direction[i] * direction[i];

        norm =
            Math.Sqrt(
                Math.Max(
                    norm,
                    1e-15));

        double d1 = 0.0;

        for (int i = 0; i < direction.Length; i++)
            d1 +=
                normalizedObjectives[i] *
                direction[i] /
                norm;

        double d2Squared = 0.0;

        for (int i = 0; i < direction.Length; i++)
        {
            double projection =
                d1 *
                direction[i] /
                norm;

            double delta =
                normalizedObjectives[i] -
                projection;

            d2Squared += delta * delta;
        }

        return d1 +
            theta *
            Math.Sqrt(d2Squared);
    }

    public static double[] IdealPoint(
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        double[] ideal =
            Enumerable.Repeat(
                double.PositiveInfinity,
                senses.Count)
            .ToArray();

        foreach (MoCandidate candidate in population)
            for (int objective = 0;
                 objective < senses.Count;
                 objective++)
                ideal[objective] =
                    Math.Min(
                        ideal[objective],
                        MultiobjectiveToolkit.Normalize(
                            candidate.Objectives[objective],
                            senses[objective]));

        return ideal;
    }

    public static double[] RelativeObjectives(
        MoCandidate candidate,
        ReadOnlySpan<double> ideal,
        IReadOnlyList<OptimizationSense> senses)
    {
        double[] result =
            new double[senses.Count];

        for (int objective = 0;
             objective < senses.Count;
             objective++)
            result[objective] =
                Math.Max(
                    0.0,
                    MultiobjectiveToolkit.Normalize(
                        candidate.Objectives[objective],
                        senses[objective]) -
                    ideal[objective]);

        return result;
    }

    public static double Determinant(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double[,] work =
            (double[,])matrix.Clone();

        double determinant = 1.0;

        for (int column = 0; column < n; column++)
        {
            int pivot = column;

            for (int row = column + 1; row < n; row++)
                if (Math.Abs(work[row, column]) >
                    Math.Abs(work[pivot, column]))
                    pivot = row;

            if (Math.Abs(work[pivot, column]) <= 1e-15)
                return 0.0;

            if (pivot != column)
            {
                for (int j = 0; j < n; j++)
                {
                    double temporary =
                        work[column, j];

                    work[column, j] =
                        work[pivot, j];

                    work[pivot, j] =
                        temporary;
                }

                determinant =
                    -determinant;
            }

            double diagonal =
                work[column, column];

            determinant *= diagonal;

            for (int row = column + 1; row < n; row++)
            {
                double factor =
                    work[row, column] /
                    diagonal;

                for (int j = column + 1; j < n; j++)
                    work[row, j] -=
                        factor *
                        work[column, j];
            }
        }

        return determinant;
    }

    public static double[,] Cholesky(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double[,] lower = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum =
                    matrix[i, j];

                for (int k = 0; k < j; k++)
                    sum -=
                        lower[i, k] *
                        lower[j, k];

                if (i == j)
                    lower[i, j] =
                        Math.Sqrt(
                            Math.Max(
                                sum,
                                1e-12));
                else
                    lower[i, j] =
                        sum /
                        Math.Max(
                            lower[j, j],
                            1e-12);
            }
        }

        return lower;
    }

    public static double NextGaussian(IRandomSource random)
    {
        double u1 =
            Math.Max(
                random.NextDouble(),
                1e-15);

        double u2 =
            random.NextDouble();

        return
            Math.Sqrt(
                -2.0 *
                Math.Log(u1)) *
            Math.Cos(
                2.0 *
                Math.PI *
                u2);
    }

    private static double AverageClusterDistance(
        IReadOnlyList<MoCandidate> first,
        IReadOnlyList<MoCandidate> second,
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        double total = 0.0;
        int count = 0;

        foreach (MoCandidate a in first)
            foreach (MoCandidate b in second)
            {
                total +=
                    ObjectiveDistance(
                        a,
                        b,
                        population,
                        senses);

                count++;
            }

        return total /
            Math.Max(
                count,
                1);
    }

    private static bool LexicographicallySmaller(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second)
    {
        int length =
            Math.Min(
                first.Count,
                second.Count);

        for (int i = 0; i < length; i++)
        {
            if (first[i] < second[i] - 1e-15)
                return true;

            if (first[i] > second[i] + 1e-15)
                return false;
        }

        return first.Count < second.Count;
    }
}
