using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Fixed-capacity quality/diversity elite pool for GRASP path relinking.
/// Stored solutions are owned clones.
/// </summary>
public sealed class EliteSolutionPool<TSolution>
{
    private readonly TSolution[] _solutions;
    private readonly double[] _fitness;
    private readonly int _minimumDistance;
    private readonly IPathRelinkingDistance<TSolution> _distance;
    private readonly IOptimizationProblem<TSolution> _problem;
    private readonly ISolutionCloner<TSolution> _solutionCloner;
    private int _count;

    /// <summary>Creates a fixed-capacity elite pool.</summary>
    public EliteSolutionPool(
        int capacity,
        int minimumDistance,
        IPathRelinkingDistance<TSolution> distance,
        IOptimizationProblem<TSolution> problem,
        ISolutionCloner<TSolution> solutionCloner)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (minimumDistance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumDistance));
        }

        _distance =
            distance ?? throw new ArgumentNullException(nameof(distance));
        _problem =
            problem ?? throw new ArgumentNullException(nameof(problem));
        _solutionCloner =
            solutionCloner ?? throw new ArgumentNullException(nameof(solutionCloner));

        _minimumDistance = minimumDistance;
        _solutions = new TSolution[capacity];
        _fitness = new double[capacity];
    }

    public int Capacity => _solutions.Length;

    public int Count => _count;

    /// <summary>
    /// Ordinary online GRASP elite admission. When full, a better diverse candidate
    /// replaces the current worst retained elite.
    /// </summary>
    public bool TryAdd(
        in TSolution solution,
        double fitness,
        out bool replaced)
    {
        ValidateFitness(fitness);
        replaced = false;

        int exactDuplicate =
            FindExactDuplicate(in solution);

        if (exactDuplicate >= 0)
        {
            return ReplaceDuplicateIfBetter(
                exactDuplicate,
                in solution,
                fitness,
                out replaced);
        }

        if (_count < Capacity)
        {
            if (!IsSufficientlyDiverse(
                    in solution,
                    excludedIndex: -1))
            {
                return false;
            }

            AddOwned(in solution, fitness);
            return true;
        }

        int worstIndex =
            FindWorstIndex();

        if (!_problem.Sense.IsBetter(
                fitness,
                _fitness[worstIndex]))
        {
            return false;
        }

        if (!IsSufficientlyDiverse(
                in solution,
                worstIndex))
        {
            return false;
        }

        ReplaceOwned(
            worstIndex,
            in solution,
            fitness);

        replaced = true;
        return true;
    }

    /// <summary>
    /// Evolutionary path-relinking population admission following the
    /// Resende-Werneck quality/diversity replacement policy.
    /// </summary>
    public bool TryAddEvolutionary(
        in TSolution solution,
        double fitness,
        out bool replaced)
    {
        ValidateFitness(fitness);
        replaced = false;

        int exactDuplicate =
            FindExactDuplicate(in solution);

        if (exactDuplicate >= 0)
        {
            return ReplaceDuplicateIfBetter(
                exactDuplicate,
                in solution,
                fitness,
                out replaced);
        }

        if (_count < Capacity)
        {
            if (!IsSufficientlyDiverse(
                    in solution,
                    excludedIndex: -1))
            {
                return false;
            }

            AddOwned(in solution, fitness);
            return true;
        }

        int bestIndex =
            FindBestIndex();
        int worstIndex =
            FindWorstIndex();

        bool improvesBest =
            _problem.Sense.IsBetter(
                fitness,
                _fitness[bestIndex]);

        bool improvesWorst =
            _problem.Sense.IsBetter(
                fitness,
                _fitness[worstIndex]);

        if (!improvesBest && !improvesWorst)
        {
            return false;
        }

        if (!improvesBest &&
            !IsSufficientlyDiverse(
                in solution,
                excludedIndex: -1))
        {
            return false;
        }

        int replacementIndex = -1;
        int closestDistance = int.MaxValue;

        for (int i = 0; i < _count; i++)
        {
            if (_problem.Sense.IsBetter(
                    _fitness[i],
                    fitness))
            {
                continue;
            }

            int distance =
                GetValidatedDistance(
                    in solution,
                    in _solutions[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                replacementIndex = i;
            }
        }

        if (replacementIndex < 0)
        {
            return false;
        }

        ReplaceOwned(
            replacementIndex,
            in solution,
            fitness);

        replaced = true;
        return true;
    }

    /// <summary>Returns the best retained elite and its stored objective value.</summary>
    public bool TryGetBest(
        out TSolution solution,
        out double fitness)
    {
        if (_count == 0)
        {
            solution = default!;
            fitness = double.NaN;
            return false;
        }

        int index =
            FindBestIndex();

        solution = _solutions[index];
        fitness = _fitness[index];
        return true;
    }

    /// <summary>
    /// Returns one retained elite by slot index. Treat the returned solution as read-only.
    /// </summary>
    public void GetAt(
        int index,
        out TSolution solution,
        out double fitness)
    {
        if ((uint)index >= (uint)_count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        solution = _solutions[index];
        fitness = _fitness[index];
    }

    /// <summary>
    /// Selects uniformly among elite solutions distinct from the initiating solution.
    /// Reservoir sampling avoids a temporary candidate list.
    /// </summary>
    public bool TrySelectGuide(
        in TSolution initiatingSolution,
        IRandomSource random,
        out TSolution guidingSolution) =>
        TrySelectGuide(
            in initiatingSolution,
            random,
            out guidingSolution,
            out _);

    /// <summary>
    /// Selects a distinct guide and returns the objective value already stored with it.
    /// </summary>
    public bool TrySelectGuide(
        in TSolution initiatingSolution,
        IRandomSource random,
        out TSolution guidingSolution,
        out double guidingFitness)
    {
        ArgumentNullException.ThrowIfNull(random);

        int eligible = 0;
        int selectedIndex = -1;

        for (int i = 0; i < _count; i++)
        {
            if (GetValidatedDistance(
                    in initiatingSolution,
                    in _solutions[i]) == 0)
            {
                continue;
            }

            eligible++;

            if (random.NextInt32(eligible) == 0)
            {
                selectedIndex = i;
            }
        }

        if (selectedIndex < 0)
        {
            guidingSolution = default!;
            guidingFitness = double.NaN;
            return false;
        }

        guidingSolution = _solutions[selectedIndex];
        guidingFitness = _fitness[selectedIndex];
        return true;
    }

    internal EliteSolutionPool<TSolution> CreateEmptySibling() =>
        new(
            Capacity,
            _minimumDistance,
            _distance,
            _problem,
            _solutionCloner);

    private int FindExactDuplicate(
        in TSolution solution)
    {
        for (int i = 0; i < _count; i++)
        {
            if (GetValidatedDistance(
                    in solution,
                    in _solutions[i]) == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private int FindBestIndex()
    {
        int bestIndex = 0;

        for (int i = 1; i < _count; i++)
        {
            if (_problem.Sense.IsBetter(
                    _fitness[i],
                    _fitness[bestIndex]))
            {
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private int FindWorstIndex()
    {
        int worstIndex = 0;

        for (int i = 1; i < _count; i++)
        {
            if (_problem.Sense.IsBetter(
                    _fitness[worstIndex],
                    _fitness[i]))
            {
                worstIndex = i;
            }
        }

        return worstIndex;
    }

    private bool IsSufficientlyDiverse(
        in TSolution solution,
        int excludedIndex)
    {
        for (int i = 0; i < _count; i++)
        {
            if (i == excludedIndex)
            {
                continue;
            }

            if (GetValidatedDistance(
                    in solution,
                    in _solutions[i]) < _minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    private bool ReplaceDuplicateIfBetter(
        int duplicateIndex,
        in TSolution solution,
        double fitness,
        out bool replaced)
    {
        replaced = false;

        if (!_problem.Sense.IsBetter(
                fitness,
                _fitness[duplicateIndex]))
        {
            return false;
        }

        ReplaceOwned(
            duplicateIndex,
            in solution,
            fitness);

        replaced = true;
        return true;
    }

    private void AddOwned(
        in TSolution solution,
        double fitness)
    {
        _solutions[_count] =
            _solutionCloner.Clone(solution);
        _fitness[_count] = fitness;
        _count++;
    }

    private void ReplaceOwned(
        int index,
        in TSolution solution,
        double fitness)
    {
        _solutions[index] =
            _solutionCloner.Clone(solution);
        _fitness[index] = fitness;
    }

    private static void ValidateFitness(
        double fitness)
    {
        if (double.IsNaN(fitness))
        {
            throw new ArgumentOutOfRangeException(nameof(fitness));
        }
    }

    private int GetValidatedDistance(
        in TSolution first,
        in TSolution second)
    {
        int distance =
            _distance.GetDistance(
                in first,
                in second,
                _problem);

        if (distance < 0)
        {
            throw new InvalidOperationException(
                "Elite-pool distance must be non-negative.");
        }

        return distance;
    }
}