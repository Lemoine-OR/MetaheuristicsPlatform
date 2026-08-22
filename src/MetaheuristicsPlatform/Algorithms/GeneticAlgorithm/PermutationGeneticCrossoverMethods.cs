using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>Partially mapped crossover (PMX) for permutation arrays.</summary>
public sealed class PartiallyMappedGeneticCrossoverMethod<T> :
    IGeneticCrossoverMethod<T[]>
    where T : notnull
{
    private readonly IEqualityComparer<T> _comparer;

    public PartiallyMappedGeneticCrossoverMethod(
        IEqualityComparer<T>? comparer = null)
    {
        _comparer =
            comparer ?? EqualityComparer<T>.Default;
    }

    public GeneticOffspringPair<T[]> Crossover(
        T[] firstParent,
        T[] secondParent,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        PermutationGeneticCrossoverUtilities.ValidateParents(
            firstParent,
            secondParent,
            random,
            _comparer);

        if (firstParent.Length < 2)
        {
            return new(
                (T[])firstParent.Clone(),
                (T[])secondParent.Clone());
        }

        int left =
            random.NextInt32(firstParent.Length - 1);

        int right =
            random.NextInt32(
                left + 1,
                firstParent.Length + 1);

        T[] firstChild = new T[firstParent.Length];
        T[] secondChild = new T[firstParent.Length];

        Array.Copy(firstParent, left, firstChild, left, right - left);
        Array.Copy(secondParent, left, secondChild, left, right - left);

        var firstSegment =
            new HashSet<T>(
                firstParent[left..right],
                _comparer);

        var secondSegment =
            new HashSet<T>(
                secondParent[left..right],
                _comparer);

        var firstToSecond =
            new Dictionary<T,T>(_comparer);

        var secondToFirst =
            new Dictionary<T,T>(_comparer);

        for (int index = left;
             index < right;
             index++)
        {
            firstToSecond[firstParent[index]] = secondParent[index];
            secondToFirst[secondParent[index]] = firstParent[index];
        }

        for (int index = 0;
             index < firstParent.Length;
             index++)
        {
            if (index >= left && index < right)
                continue;

            T firstCandidate = secondParent[index];
            int firstGuard = 0;

            while (firstSegment.Contains(firstCandidate))
            {
                firstCandidate = firstToSecond[firstCandidate];

                if (++firstGuard > firstParent.Length)
                {
                    throw new InvalidOperationException(
                        "PMX mapping did not leave the copied segment.");
                }
            }

            firstChild[index] = firstCandidate;

            T secondCandidate = firstParent[index];
            int secondGuard = 0;

            while (secondSegment.Contains(secondCandidate))
            {
                secondCandidate = secondToFirst[secondCandidate];

                if (++secondGuard > firstParent.Length)
                {
                    throw new InvalidOperationException(
                        "PMX mapping did not leave the copied segment.");
                }
            }

            secondChild[index] = secondCandidate;
        }

        return new(firstChild, secondChild);
    }
}

/// <summary>Order crossover OX1 for permutation arrays.</summary>
public sealed class OrderGeneticCrossoverMethod<T> :
    IGeneticCrossoverMethod<T[]>
    where T : notnull
{
    private readonly IEqualityComparer<T> _comparer;

    public OrderGeneticCrossoverMethod(
        IEqualityComparer<T>? comparer = null)
    {
        _comparer =
            comparer ?? EqualityComparer<T>.Default;
    }

    public GeneticOffspringPair<T[]> Crossover(
        T[] firstParent,
        T[] secondParent,
        IOptimizationProblem<T[]> problem,
        IRandomSource random)
    {
        PermutationGeneticCrossoverUtilities.ValidateParents(
            firstParent,
            secondParent,
            random,
            _comparer);

        if (firstParent.Length < 2)
        {
            return new(
                (T[])firstParent.Clone(),
                (T[])secondParent.Clone());
        }

        int left =
            random.NextInt32(firstParent.Length - 1);

        int right =
            random.NextInt32(
                left + 1,
                firstParent.Length + 1);

        T[] firstChild =
            CreateChild(
                firstParent,
                secondParent,
                left,
                right);

        T[] secondChild =
            CreateChild(
                secondParent,
                firstParent,
                left,
                right);

        return new(firstChild, secondChild);
    }

    private T[] CreateChild(
        T[] segmentParent,
        T[] orderParent,
        int left,
        int right)
    {
        int length = segmentParent.Length;
        T[] child = new T[length];
        bool[] assigned = new bool[length];

        var used =
            new HashSet<T>(_comparer);

        for (int index = left;
             index < right;
             index++)
        {
            child[index] = segmentParent[index];
            assigned[index] = true;
            used.Add(segmentParent[index]);
        }

        int write = right % length;

        for (int offset = 0;
             offset < length;
             offset++)
        {
            T candidate =
                orderParent[(right + offset) % length];

            if (used.Contains(candidate))
                continue;

            while (assigned[write])
                write = (write + 1) % length;

            child[write] = candidate;
            assigned[write] = true;
            used.Add(candidate);
            write = (write + 1) % length;
        }

        return child;
    }
}

internal static class PermutationGeneticCrossoverUtilities
{
    public static void ValidateParents<T>(
        T[] firstParent,
        T[] secondParent,
        IRandomSource random,
        IEqualityComparer<T> comparer)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(firstParent);
        ArgumentNullException.ThrowIfNull(secondParent);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(comparer);

        if (firstParent.Length != secondParent.Length)
        {
            throw new ArgumentException(
                "Permutation crossover requires parents with equal lengths.");
        }

        var firstSet =
            new HashSet<T>(firstParent, comparer);

        var secondSet =
            new HashSet<T>(secondParent, comparer);

        if (firstSet.Count != firstParent.Length ||
            secondSet.Count != secondParent.Length)
        {
            throw new ArgumentException(
                "Permutation crossover requires unique alleles in each parent.");
        }

        if (!firstSet.SetEquals(secondSet))
        {
            throw new ArgumentException(
                "Permutation crossover requires parents containing the same allele set.");
        }
    }
}
