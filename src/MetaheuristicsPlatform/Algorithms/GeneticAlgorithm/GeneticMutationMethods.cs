using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>Independent per-bit mutation for boolean arrays.</summary>
public sealed class BitFlipGeneticMutationMethod :
    IGeneticMutationMethod<bool[]>
{
    public BitFlipGeneticMutationMethod(
        double perBitProbability)
    {
        ValidateProbability(
            perBitProbability,
            nameof(perBitProbability));

        PerBitProbability = perBitProbability;
    }

    public double PerBitProbability { get; }

    public bool[] Mutate(
        bool[] solution,
        IOptimizationProblem<bool[]> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(random);

        for (int index = 0;
             index < solution.Length;
             index++)
        {
            if (random.NextDouble() < PerBitProbability)
                solution[index] = !solution[index];
        }

        return solution;
    }

    internal static void ValidateProbability(
        double probability,
        string parameterName)
    {
        if (!double.IsFinite(probability) ||
            probability < 0.0 ||
            probability > 1.0)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}

/// <summary>
/// Per-gene uniform random resetting for bounded integer vectors.
/// Bounds use an inclusive lower endpoint and an exclusive upper endpoint.
/// </summary>
public sealed class IntegerRandomResetGeneticMutationMethod :
    IGeneticMutationMethod<int[]>
{
    private readonly int[] _inclusiveLowerBounds;
    private readonly int[] _exclusiveUpperBounds;

    public IntegerRandomResetGeneticMutationMethod(
        IReadOnlyList<int> inclusiveLowerBounds,
        IReadOnlyList<int> exclusiveUpperBounds,
        double perGeneProbability)
    {
        ArgumentNullException.ThrowIfNull(inclusiveLowerBounds);
        ArgumentNullException.ThrowIfNull(exclusiveUpperBounds);

        if (inclusiveLowerBounds.Count != exclusiveUpperBounds.Count)
        {
            throw new ArgumentException(
                "Integer mutation bound vectors must have equal lengths.");
        }

        BitFlipGeneticMutationMethod.ValidateProbability(
            perGeneProbability,
            nameof(perGeneProbability));

        _inclusiveLowerBounds = inclusiveLowerBounds.ToArray();
        _exclusiveUpperBounds = exclusiveUpperBounds.ToArray();

        for (int index = 0;
             index < _inclusiveLowerBounds.Length;
             index++)
        {
            if (_exclusiveUpperBounds[index] <=
                _inclusiveLowerBounds[index])
            {
                throw new ArgumentException(
                    "Each integer mutation interval must be non-empty.");
            }

            long intervalWidth =
                (long)_exclusiveUpperBounds[index] -
                _inclusiveLowerBounds[index];

            if (intervalWidth > int.MaxValue)
            {
                throw new ArgumentException(
                    "Each integer mutation interval width must fit the IRandomSource.NextInt32 contract.");
            }
        }

        PerGeneProbability = perGeneProbability;
    }

    public double PerGeneProbability { get; }

    public int[] Mutate(
        int[] solution,
        IOptimizationProblem<int[]> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(random);

        if (solution.Length != _inclusiveLowerBounds.Length)
        {
            throw new ArgumentException(
                "Integer solution and bound vectors must have equal lengths.",
                nameof(solution));
        }

        for (int index = 0;
             index < solution.Length;
             index++)
        {
            int lower = _inclusiveLowerBounds[index];
            int upper = _exclusiveUpperBounds[index];
            int current = solution[index];

            if (current < lower || current >= upper)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(solution),
                    "Integer mutation requires the current solution inside configured bounds.");
            }

            if (random.NextDouble() >= PerGeneProbability)
                continue;

            int range = checked(upper - lower);

            if (range <= 1)
                continue;

            int currentOffset = current - lower;
            int sampled = random.NextInt32(range - 1);
            int newOffset =
                sampled >= currentOffset
                    ? sampled + 1
                    : sampled;

            solution[index] = lower + newOffset;
        }

        return solution;
    }
}

/// <summary>Swaps two distinct positions in an array once per mutation invocation.</summary>
public sealed class SwapGeneticMutationMethod<T> :
    IGeneticMutationMethod<T[]>
{
    public T[] Mutate(
        T[] solution,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(random);

        if (solution.Length < 2)
            return solution;

        int first = random.NextInt32(solution.Length);
        int second = random.NextInt32(solution.Length - 1);

        if (second >= first)
            second++;

        (solution[first], solution[second]) =
            (solution[second], solution[first]);

        return solution;
    }
}

/// <summary>Reverses one non-empty contiguous segment once per mutation invocation.</summary>
public sealed class InversionGeneticMutationMethod<T> :
    IGeneticMutationMethod<T[]>
{
    public T[] Mutate(
        T[] solution,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(random);

        if (solution.Length < 2)
            return solution;

        int left = random.NextInt32(solution.Length - 1);
        int right =
            random.NextInt32(
                left + 1,
                solution.Length + 1);

        int low = left;
        int high = right - 1;

        while (low < high)
        {
            (solution[low], solution[high]) =
                (solution[high], solution[low]);
            low++;
            high--;
        }

        return solution;
    }
}

/// <summary>
/// Independent bounded Gaussian mutation for real vectors, using projection to configured bounds.
/// </summary>
public sealed class BoundedGaussianGeneticMutationMethod :
    IGeneticMutationMethod<double[]>
{
    private readonly double[] _lowerBounds;
    private readonly double[] _upperBounds;

    public BoundedGaussianGeneticMutationMethod(
        IReadOnlyList<double> lowerBounds,
        IReadOnlyList<double> upperBounds,
        double standardDeviation,
        double perGeneProbability)
    {
        (_lowerBounds, _upperBounds) =
            RealMutationUtilities.CopyAndValidateBounds(
                lowerBounds,
                upperBounds);

        if (!double.IsFinite(standardDeviation) || standardDeviation < 0.0)
            throw new ArgumentOutOfRangeException(nameof(standardDeviation));

        BitFlipGeneticMutationMethod.ValidateProbability(
            perGeneProbability,
            nameof(perGeneProbability));

        StandardDeviation = standardDeviation;
        PerGeneProbability = perGeneProbability;
    }

    public double StandardDeviation { get; }
    public double PerGeneProbability { get; }

    public double[] Mutate(
        double[] solution,
        IOptimizationProblem<double[]> problem,
        IRandomSource random)
    {
        RealMutationUtilities.ValidateSolution(
            solution,
            _lowerBounds,
            _upperBounds,
            random);

        for (int index = 0;
             index < solution.Length;
             index++)
        {
            if (random.NextDouble() >= PerGeneProbability)
                continue;

            if (_lowerBounds[index] == _upperBounds[index])
            {
                solution[index] = _lowerBounds[index];
                continue;
            }

            double z =
                RealMutationUtilities.NextStandardNormal(random);

            solution[index] =
                Math.Clamp(
                    solution[index] + StandardDeviation * z,
                    _lowerBounds[index],
                    _upperBounds[index]);
        }

        return solution;
    }
}

/// <summary>Bounded polynomial mutation for real vectors.</summary>
public sealed class BoundedPolynomialGeneticMutationMethod :
    IGeneticMutationMethod<double[]>
{
    private readonly double[] _lowerBounds;
    private readonly double[] _upperBounds;

    public BoundedPolynomialGeneticMutationMethod(
        IReadOnlyList<double> lowerBounds,
        IReadOnlyList<double> upperBounds,
        double distributionIndex = 20.0,
        double perGeneProbability = 0.01)
    {
        (_lowerBounds, _upperBounds) =
            RealMutationUtilities.CopyAndValidateBounds(
                lowerBounds,
                upperBounds);

        if (!double.IsFinite(distributionIndex) || distributionIndex < 0.0)
            throw new ArgumentOutOfRangeException(nameof(distributionIndex));

        BitFlipGeneticMutationMethod.ValidateProbability(
            perGeneProbability,
            nameof(perGeneProbability));

        DistributionIndex = distributionIndex;
        PerGeneProbability = perGeneProbability;
    }

    public double DistributionIndex { get; }
    public double PerGeneProbability { get; }

    public double[] Mutate(
        double[] solution,
        IOptimizationProblem<double[]> problem,
        IRandomSource random)
    {
        RealMutationUtilities.ValidateSolution(
            solution,
            _lowerBounds,
            _upperBounds,
            random);

        double mutationPower =
            1.0 / (DistributionIndex + 1.0);

        for (int index = 0;
             index < solution.Length;
             index++)
        {
            if (random.NextDouble() >= PerGeneProbability)
                continue;

            double lower = _lowerBounds[index];
            double upper = _upperBounds[index];

            if (lower == upper)
            {
                solution[index] = lower;
                continue;
            }

            double value = solution[index];
            double range = upper - lower;
            double delta1 = (value - lower) / range;
            double delta2 = (upper - value) / range;
            double randomValue = random.NextDouble();
            double deltaQ;

            if (randomValue <= 0.5)
            {
                double xy = 1.0 - delta1;
                double term =
                    2.0 * randomValue +
                    (1.0 - 2.0 * randomValue) *
                    Math.Pow(xy, DistributionIndex + 1.0);

                deltaQ =
                    Math.Pow(term, mutationPower) - 1.0;
            }
            else
            {
                double xy = 1.0 - delta2;
                double term =
                    2.0 * (1.0 - randomValue) +
                    2.0 * (randomValue - 0.5) *
                    Math.Pow(xy, DistributionIndex + 1.0);

                deltaQ =
                    1.0 - Math.Pow(term, mutationPower);
            }

            solution[index] =
                Math.Clamp(
                    value + deltaQ * range,
                    lower,
                    upper);
        }

        return solution;
    }
}

internal static class RealMutationUtilities
{
    public static (double[] Lower,double[] Upper) CopyAndValidateBounds(
        IReadOnlyList<double> lowerBounds,
        IReadOnlyList<double> upperBounds)
    {
        ArgumentNullException.ThrowIfNull(lowerBounds);
        ArgumentNullException.ThrowIfNull(upperBounds);

        if (lowerBounds.Count != upperBounds.Count)
        {
            throw new ArgumentException(
                "Real mutation bound vectors must have equal lengths.");
        }

        double[] lower = lowerBounds.ToArray();
        double[] upper = upperBounds.ToArray();

        for (int index = 0;
             index < lower.Length;
             index++)
        {
            if (!double.IsFinite(lower[index]) ||
                !double.IsFinite(upper[index]) ||
                lower[index] > upper[index])
            {
                throw new ArgumentException(
                    "Real mutation bounds must be finite and lower <= upper.");
            }
        }

        return (lower, upper);
    }

    public static void ValidateSolution(
        double[] solution,
        IReadOnlyList<double> lowerBounds,
        IReadOnlyList<double> upperBounds,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(random);

        if (solution.Length != lowerBounds.Count)
        {
            throw new ArgumentException(
                "Real solution and bound vectors must have equal lengths.",
                nameof(solution));
        }

        for (int index = 0;
             index < solution.Length;
             index++)
        {
            if (!double.IsFinite(solution[index]) ||
                solution[index] < lowerBounds[index] ||
                solution[index] > upperBounds[index])
            {
                throw new ArgumentOutOfRangeException(
                    nameof(solution),
                    "Real mutation requires a finite solution inside configured bounds.");
            }
        }
    }

    public static double NextStandardNormal(
        IRandomSource random)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = random.NextDouble();

        return Math.Sqrt(-2.0 * Math.Log(u1)) *
               Math.Cos(2.0 * Math.PI * u2);
    }
}
