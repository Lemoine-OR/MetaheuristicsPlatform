using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Bounded Simulated Binary Crossover (SBX) for real-valued arrays.
/// The bounded formulas follow the widely used NSGA-II form while preserving the original SBX semantics.
/// </summary>
public sealed class BoundedSimulatedBinaryGeneticCrossoverMethod :
    IGeneticCrossoverMethod<double[]>
{
    private readonly double[] _lowerBounds;
    private readonly double[] _upperBounds;

    public BoundedSimulatedBinaryGeneticCrossoverMethod(
        IReadOnlyList<double> lowerBounds,
        IReadOnlyList<double> upperBounds,
        double distributionIndex = 20.0,
        double perVariableCrossoverProbability = 0.5)
    {
        ArgumentNullException.ThrowIfNull(lowerBounds);
        ArgumentNullException.ThrowIfNull(upperBounds);

        if (lowerBounds.Count != upperBounds.Count)
        {
            throw new ArgumentException(
                "Lower and upper bound vectors must have equal lengths.");
        }

        if (!double.IsFinite(distributionIndex) || distributionIndex < 0.0)
            throw new ArgumentOutOfRangeException(nameof(distributionIndex));

        if (!double.IsFinite(perVariableCrossoverProbability) ||
            perVariableCrossoverProbability < 0.0 ||
            perVariableCrossoverProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(perVariableCrossoverProbability));
        }

        _lowerBounds = lowerBounds.ToArray();
        _upperBounds = upperBounds.ToArray();

        for (int index = 0;
             index < _lowerBounds.Length;
             index++)
        {
            if (!double.IsFinite(_lowerBounds[index]) ||
                !double.IsFinite(_upperBounds[index]) ||
                _lowerBounds[index] > _upperBounds[index])
            {
                throw new ArgumentException(
                    "Bounds must be finite and lower <= upper.");
            }
        }

        DistributionIndex = distributionIndex;
        PerVariableCrossoverProbability =
            perVariableCrossoverProbability;
    }

    public double DistributionIndex { get; }
    public double PerVariableCrossoverProbability { get; }

    public GeneticOffspringPair<double[]> Crossover(
        double[] firstParent,
        double[] secondParent,
        IOptimizationProblem<double[]> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(firstParent);
        ArgumentNullException.ThrowIfNull(secondParent);
        ArgumentNullException.ThrowIfNull(random);

        if (firstParent.Length != secondParent.Length ||
            firstParent.Length != _lowerBounds.Length)
        {
            throw new ArgumentException(
                "SBX parents and bound vectors must have equal lengths.");
        }

        ValidateInsideBounds(firstParent);
        ValidateInsideBounds(secondParent);

        double[] firstChild = (double[])firstParent.Clone();
        double[] secondChild = (double[])secondParent.Clone();

        double exponent =
            1.0 / (DistributionIndex + 1.0);

        for (int index = 0;
             index < firstParent.Length;
             index++)
        {
            if (random.NextDouble() >= PerVariableCrossoverProbability)
                continue;

            double parentA = firstParent[index];
            double parentB = secondParent[index];
            double lower = _lowerBounds[index];
            double upper = _upperBounds[index];

            if (lower == upper ||
                Math.Abs(parentA - parentB) <= 1e-14)
            {
                firstChild[index] = lower == upper ? lower : parentA;
                secondChild[index] = lower == upper ? lower : parentB;
                continue;
            }

            double y1 = Math.Min(parentA, parentB);
            double y2 = Math.Max(parentA, parentB);
            double randomValue = random.NextDouble();

            double beta =
                1.0 +
                2.0 * (y1 - lower) / (y2 - y1);

            double alpha =
                2.0 -
                Math.Pow(beta, -(DistributionIndex + 1.0));

            double betaQ =
                randomValue <= 1.0 / alpha
                    ? Math.Pow(randomValue * alpha, exponent)
                    : Math.Pow(1.0 / (2.0 - randomValue * alpha), exponent);

            double childLow =
                0.5 *
                ((y1 + y2) - betaQ * (y2 - y1));

            beta =
                1.0 +
                2.0 * (upper - y2) / (y2 - y1);

            alpha =
                2.0 -
                Math.Pow(beta, -(DistributionIndex + 1.0));

            betaQ =
                randomValue <= 1.0 / alpha
                    ? Math.Pow(randomValue * alpha, exponent)
                    : Math.Pow(1.0 / (2.0 - randomValue * alpha), exponent);

            double childHigh =
                0.5 *
                ((y1 + y2) + betaQ * (y2 - y1));

            childLow = Math.Clamp(childLow, lower, upper);
            childHigh = Math.Clamp(childHigh, lower, upper);

            if (random.NextDouble() < 0.5)
            {
                firstChild[index] = childHigh;
                secondChild[index] = childLow;
            }
            else
            {
                firstChild[index] = childLow;
                secondChild[index] = childHigh;
            }
        }

        return new(firstChild, secondChild);
    }

    private void ValidateInsideBounds(
        IReadOnlyList<double> values)
    {
        for (int index = 0;
             index < values.Count;
             index++)
        {
            double value = values[index];

            if (!double.IsFinite(value) ||
                value < _lowerBounds[index] ||
                value > _upperBounds[index])
            {
                throw new ArgumentOutOfRangeException(
                    nameof(values),
                    "SBX requires finite parents inside the configured bounds.");
            }
        }
    }
}
