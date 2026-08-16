using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Fixed-capacity quality/diversity elite pool for GRASP path relinking.
/// Stored solutions are owned clones. Exact duplicates can be replaced by a better copy;
/// when full, a better candidate may replace the current worst elite if diversity with
/// all surviving elites is preserved.
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

    /// <summary>Maximum number of elite solutions retained.</summary>
    public int Capacity => _solutions.Length;

    /// <summary>Current number of elite solutions.</summary>
    public int Count => _count;

    /// <summary>
    /// Attempts to insert a candidate while preserving the configured minimum distance.
    /// </summary>
    public bool TryAdd(
        in TSolution solution,
        double fitness,
        out bool replaced)
    {
        if (double.IsNaN(fitness))
        {
            throw new ArgumentOutOfRangeException(nameof(fitness));
        }

        replaced = false;

        int exactDuplicate = -1;

        for (int i = 0; i < _count; i++)
        {
            int distance =
                GetValidatedDistance(
                    in solution,
                    in _solutions[i]);

            if (distance == 0)
            {
                exactDuplicate = i;
                break;
            }
        }

        if (exactDuplicate >= 0)
        {
            if (!_problem.Sense.IsBetter(
                    fitness,
                    _fitness[exactDuplicate]))
            {
                return false;
            }

            _solutions[exactDuplicate] =
                _solutionCloner.Clone(solution);
            _fitness[exactDuplicate] = fitness;
            replaced = true;
            return true;
        }

        if (_count < Capacity)
        {
            for (int i = 0; i < _count; i++)
            {
                if (GetValidatedDistance(
                        in solution,
                        in _solutions[i]) < _minimumDistance)
                {
                    return false;
                }
            }

            _solutions[_count] =
                _solutionCloner.Clone(solution);
            _fitness[_count] = fitness;
            _count++;
            return true;
        }

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

        if (!_problem.Sense.IsBetter(
                fitness,
                _fitness[worstIndex]))
        {
            return false;
        }

        for (int i = 0; i < _count; i++)
        {
            if (i == worstIndex)
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

        _solutions[worstIndex] =
            _solutionCloner.Clone(solution);
        _fitness[worstIndex] = fitness;
        replaced = true;
        return true;
    }

    /// <summary>
    /// Selects uniformly among elite solutions distinct from the initiating solution.
    /// Reservoir sampling avoids a temporary candidate list.
    /// </summary>
    public bool TrySelectGuide(
        in TSolution initiatingSolution,
        IRandomSource random,
        out TSolution guidingSolution)
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
            return false;
        }

        guidingSolution = _solutions[selectedIndex];
        return true;
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