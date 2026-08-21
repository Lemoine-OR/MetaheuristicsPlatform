using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>
/// Classical simple RefSet rule:
/// initial best-quality tier + max-min diversity tier,
/// followed by strict quality replacement of the current worst distinct member.
/// </summary>
public sealed class ClassicalScatterSearchReferenceSetUpdateMethod<TSolution> :
    IScatterSearchReferenceSetUpdateMethod<TSolution>
{
    public ClassicalScatterSearchReferenceSetUpdateMethod(
        double duplicateDistanceTolerance = 0.0)
    {
        if (!double.IsFinite(duplicateDistanceTolerance) ||
            duplicateDistanceTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duplicateDistanceTolerance));
        }

        DuplicateDistanceTolerance =
            duplicateDistanceTolerance;
    }

    public double DuplicateDistanceTolerance { get; }

    public void Initialize(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> population,
        int referenceSetSize,
        int qualityReferenceSetSize,
        IScatterSearchDistance<TSolution> distance,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner)
    {
        ArgumentNullException.ThrowIfNull(referenceSet);
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(distance);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        if (referenceSetSize < 2)
            throw new ArgumentOutOfRangeException(nameof(referenceSetSize));

        if (qualityReferenceSetSize <= 0 ||
            qualityReferenceSetSize >= referenceSetSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(qualityReferenceSetSize));
        }

        if (population.Count < referenceSetSize)
        {
            throw new ArgumentException(
                "The diversified population is smaller than the requested RefSet.",
                nameof(population));
        }

        referenceSet.Clear();

        var ordered =
            population
                .OrderBy(
                    point => point,
                    Comparer<ScatterSearchReferencePoint<TSolution>>.Create(
                        (left, right) =>
                            CompareObjectives(
                                left.Objective,
                                right.Objective,
                                sense)))
                .ToList();

        var remaining =
            new List<ScatterSearchReferencePoint<TSolution>>(ordered);

        foreach (ScatterSearchReferencePoint<TSolution> candidate in ordered)
        {
            if (referenceSet.Count >= qualityReferenceSetSize)
                break;

            if (IsDuplicate(
                    referenceSet,
                    candidate.Solution,
                    distance))
            {
                continue;
            }

            referenceSet.Add(
                ClonePoint(
                    candidate,
                    solutionCloner));

            remaining.Remove(candidate);
        }

        if (referenceSet.Count < qualityReferenceSetSize)
        {
            throw new InvalidOperationException(
                "The diversified population does not contain enough distinct solutions for the quality tier.");
        }

        while (referenceSet.Count < referenceSetSize)
        {
            ScatterSearchReferencePoint<TSolution>? selected =
                null;

            double selectedDistance =
                double.NegativeInfinity;

            foreach (ScatterSearchReferencePoint<TSolution> candidate in remaining)
            {
                if (IsDuplicate(
                        referenceSet,
                        candidate.Solution,
                        distance))
                {
                    continue;
                }

                double minimumDistance =
                    MinimumDistance(
                        referenceSet,
                        candidate.Solution,
                        distance);

                if (minimumDistance > selectedDistance)
                {
                    selectedDistance =
                        minimumDistance;

                    selected =
                        candidate;
                }
                else if (minimumDistance == selectedDistance &&
                         selected is not null &&
                         sense.IsBetter(
                             candidate.Objective,
                             selected.Objective))
                {
                    selected =
                        candidate;
                }
            }

            if (selected is null)
            {
                throw new InvalidOperationException(
                    "The diversified population does not contain enough distinct solutions to fill the RefSet diversity tier.");
            }

            referenceSet.Add(
                ClonePoint(
                    selected,
                    solutionCloner));

            remaining.Remove(selected);
        }
    }

    public bool TryUpdate(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        ScatterSearchReferencePoint<TSolution> candidate,
        IScatterSearchDistance<TSolution> distance,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner)
    {
        ArgumentNullException.ThrowIfNull(referenceSet);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(distance);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        if (referenceSet.Count == 0)
            throw new ArgumentException("The RefSet cannot be empty.", nameof(referenceSet));

        if (IsDuplicate(
                referenceSet,
                candidate.Solution,
                distance))
        {
            return false;
        }

        int worstIndex = 0;

        for (int i = 1; i < referenceSet.Count; i++)
        {
            if (sense.IsBetter(
                    referenceSet[worstIndex].Objective,
                    referenceSet[i].Objective))
            {
                worstIndex = i;
            }
        }

        if (!sense.IsBetter(
                candidate.Objective,
                referenceSet[worstIndex].Objective))
        {
            return false;
        }

        referenceSet[worstIndex] =
            ClonePoint(
                candidate,
                solutionCloner);

        return true;
    }

    private ScatterSearchReferencePoint<TSolution> ClonePoint(
        ScatterSearchReferencePoint<TSolution> point,
        ISolutionCloner<TSolution> solutionCloner) =>
        new(
            solutionCloner.Clone(point.Solution),
            point.Objective,
            isNew: true);

    private bool IsDuplicate(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        foreach (ScatterSearchReferencePoint<TSolution> member in referenceSet)
        {
            double value =
                CheckedDistance(
                    member.Solution,
                    candidate,
                    distance);

            if (value <= DuplicateDistanceTolerance)
                return true;
        }

        return false;
    }

    private static double MinimumDistance(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        double minimum =
            double.PositiveInfinity;

        foreach (ScatterSearchReferencePoint<TSolution> member in referenceSet)
        {
            double value =
                CheckedDistance(
                    member.Solution,
                    candidate,
                    distance);

            if (value < minimum)
                minimum = value;
        }

        return minimum;
    }

    private static double CheckedDistance(
        TSolution left,
        TSolution right,
        IScatterSearchDistance<TSolution> distance)
    {
        double value =
            distance.Distance(
                in left,
                in right);

        if (!double.IsFinite(value) ||
            value < 0.0)
        {
            throw new InvalidOperationException(
                "Scatter Search distance functions must return a finite non-negative value.");
        }

        return value;
    }

    private static int CompareObjectives(
        double left,
        double right,
        OptimizationSense sense)
    {
        if (sense.IsBetter(left, right))
            return -1;

        if (sense.IsBetter(right, left))
            return 1;

        return 0;
    }
}
