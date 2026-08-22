using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>Classical one-point crossover for equal-length array representations.</summary>
public sealed class OnePointGeneticCrossoverMethod<T> :
    IGeneticCrossoverMethod<T[]>
{
    public GeneticOffspringPair<T[]> Crossover(
        T[] firstParent,
        T[] secondParent,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        ValidateParents(firstParent, secondParent, random);

        T[] firstChild = (T[])firstParent.Clone();
        T[] secondChild = (T[])secondParent.Clone();

        if (firstParent.Length < 2)
            return new(firstChild, secondChild);

        int cut =
            random.NextInt32(1, firstParent.Length);

        for (int index = cut;
             index < firstParent.Length;
             index++)
        {
            firstChild[index] = secondParent[index];
            secondChild[index] = firstParent[index];
        }

        return new(firstChild, secondChild);
    }

    internal static void ValidateParents(
        T[] firstParent,
        T[] secondParent,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(firstParent);
        ArgumentNullException.ThrowIfNull(secondParent);
        ArgumentNullException.ThrowIfNull(random);

        if (firstParent.Length != secondParent.Length)
        {
            throw new ArgumentException(
                "Sequence crossover requires parents with equal lengths.");
        }
    }
}

/// <summary>Classical two-point crossover for equal-length array representations.</summary>
public sealed class TwoPointGeneticCrossoverMethod<T> :
    IGeneticCrossoverMethod<T[]>
{
    public GeneticOffspringPair<T[]> Crossover(
        T[] firstParent,
        T[] secondParent,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        OnePointGeneticCrossoverMethod<T>.ValidateParents(
            firstParent,
            secondParent,
            random);

        T[] firstChild = (T[])firstParent.Clone();
        T[] secondChild = (T[])secondParent.Clone();

        if (firstParent.Length < 2)
            return new(firstChild, secondChild);

        int left =
            random.NextInt32(firstParent.Length - 1);

        int right =
            random.NextInt32(
                left + 1,
                firstParent.Length + 1);

        for (int index = left;
             index < right;
             index++)
        {
            firstChild[index] = secondParent[index];
            secondChild[index] = firstParent[index];
        }

        return new(firstChild, secondChild);
    }
}

/// <summary>Uniform crossover with an explicit per-locus exchange probability.</summary>
public sealed class UniformGeneticCrossoverMethod<T> :
    IGeneticCrossoverMethod<T[]>
{
    public UniformGeneticCrossoverMethod(
        double exchangeProbability = 0.5)
    {
        if (!double.IsFinite(exchangeProbability) ||
            exchangeProbability < 0.0 ||
            exchangeProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(exchangeProbability));
        }

        ExchangeProbability = exchangeProbability;
    }

    public double ExchangeProbability { get; }

    public GeneticOffspringPair<T[]> Crossover(
        T[] firstParent,
        T[] secondParent,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        OnePointGeneticCrossoverMethod<T>.ValidateParents(
            firstParent,
            secondParent,
            random);

        T[] firstChild = (T[])firstParent.Clone();
        T[] secondChild = (T[])secondParent.Clone();

        for (int index = 0;
             index < firstParent.Length;
             index++)
        {
            if (random.NextDouble() < ExchangeProbability)
            {
                firstChild[index] = secondParent[index];
                secondChild[index] = firstParent[index];
            }
        }

        return new(firstChild, secondChild);
    }
}
