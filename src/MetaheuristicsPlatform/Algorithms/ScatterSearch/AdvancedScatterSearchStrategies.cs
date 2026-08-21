using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>
/// Controls when the subset schedule is refreshed after a RefSet admission.
/// RoundSnapshot preserves the v0.39.0 lifecycle. DynamicImmediate stops the
/// current schedule after an admission so the next combination round is built
/// from the updated RefSet.
/// </summary>
public enum ScatterSearchReferenceSetRefreshMode
{
    RoundSnapshot = 0,
    DynamicImmediate = 1
}

/// <summary>
/// Optional advanced RefSet rebuilding method invoked after a complete stable
/// combination round.
/// </summary>
public interface IScatterSearchReferenceSetRebuildingMethod<TSolution>
{
    bool TryRebuild(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> diversifiedPopulation,
        int qualityReferenceSetSize,
        IScatterSearchDistance<TSolution> distance,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner);
}

/// <summary>
/// Two-tier RefSet update preserving a quality tier and a diversity tier.
/// The quality tier is improved by objective value; the diversity tier is
/// improved by max-min distance.
/// </summary>
public sealed class TwoTierScatterSearchReferenceSetUpdateMethod<TSolution> :
    IScatterSearchReferenceSetUpdateMethod<TSolution>
{
    public TwoTierScatterSearchReferenceSetUpdateMethod(
        int qualityTierSize,
        double duplicateDistanceTolerance = 0.0,
        double minimumQualityDistance = 0.0)
    {
        if (qualityTierSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(qualityTierSize));

        if (!double.IsFinite(duplicateDistanceTolerance) ||
            duplicateDistanceTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duplicateDistanceTolerance));
        }

        if (!double.IsFinite(minimumQualityDistance) ||
            minimumQualityDistance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumQualityDistance));
        }

        QualityTierSize = qualityTierSize;
        DuplicateDistanceTolerance = duplicateDistanceTolerance;
        MinimumQualityDistance = minimumQualityDistance;
    }

    public int QualityTierSize { get; }
    public double DuplicateDistanceTolerance { get; }
    public double MinimumQualityDistance { get; }

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

        if (qualityReferenceSetSize != QualityTierSize)
        {
            throw new ArgumentException(
                "The configured quality tier must equal ScatterSearchParameters.QualityReferenceSetSize.",
                nameof(qualityReferenceSetSize));
        }

        if (referenceSetSize <= QualityTierSize)
            throw new ArgumentOutOfRangeException(nameof(referenceSetSize));

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
            if (referenceSet.Count >= QualityTierSize)
                break;

            if (IsDuplicate(
                    referenceSet,
                    candidate.Solution,
                    distance))
            {
                continue;
            }

            if (referenceSet.Count > 0 &&
                MinimumDistance(
                    referenceSet,
                    candidate.Solution,
                    distance) < MinimumQualityDistance)
            {
                continue;
            }

            referenceSet.Add(
                ClonePoint(
                    candidate,
                    solutionCloner,
                    isNew: true));

            remaining.Remove(candidate);
        }

        if (referenceSet.Count < QualityTierSize)
        {
            throw new InvalidOperationException(
                "The diversified population does not contain enough mutually admissible solutions for the quality tier.");
        }

        while (referenceSet.Count < referenceSetSize)
        {
            ScatterSearchReferencePoint<TSolution>? selected = null;
            double selectedDistance = double.NegativeInfinity;

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
                    selectedDistance = minimumDistance;
                    selected = candidate;
                }
                else if (minimumDistance == selectedDistance &&
                         selected is not null &&
                         sense.IsBetter(
                             candidate.Objective,
                             selected.Objective))
                {
                    selected = candidate;
                }
            }

            if (selected is null)
            {
                throw new InvalidOperationException(
                    "The diversified population does not contain enough distinct solutions to fill the diversity tier.");
            }

            referenceSet.Add(
                ClonePoint(
                    selected,
                    solutionCloner,
                    isNew: true));

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

        if (referenceSet.Count <= QualityTierSize)
        {
            throw new ArgumentException(
                "A two-tier RefSet must contain at least one diversity member.",
                nameof(referenceSet));
        }

        if (IsDuplicate(
                referenceSet,
                candidate.Solution,
                distance))
        {
            return false;
        }

        int worstQualityIndex = 0;

        for (int i = 1; i < QualityTierSize; i++)
        {
            if (sense.IsBetter(
                    referenceSet[worstQualityIndex].Objective,
                    referenceSet[i].Objective))
            {
                worstQualityIndex = i;
            }
        }

        if (sense.IsBetter(
                candidate.Objective,
                referenceSet[worstQualityIndex].Objective) &&
            MeetsQualityDistanceAfterReplacement(
                referenceSet,
                worstQualityIndex,
                candidate.Solution,
                distance))
        {
            referenceSet[worstQualityIndex] =
                ClonePoint(
                    candidate,
                    solutionCloner,
                    isNew: true);

            return true;
        }

        int leastDiverseIndex = QualityTierSize;
        double leastDiversity =
            MinimumDistanceExcluding(
                referenceSet,
                leastDiverseIndex,
                referenceSet[leastDiverseIndex].Solution,
                distance);

        for (int i = QualityTierSize + 1; i < referenceSet.Count; i++)
        {
            double diversity =
                MinimumDistanceExcluding(
                    referenceSet,
                    i,
                    referenceSet[i].Solution,
                    distance);

            if (diversity < leastDiversity)
            {
                leastDiversity = diversity;
                leastDiverseIndex = i;
            }
        }

        double candidateDiversity =
            MinimumDistanceExcluding(
                referenceSet,
                leastDiverseIndex,
                candidate.Solution,
                distance);

        if (candidateDiversity <= leastDiversity)
            return false;

        referenceSet[leastDiverseIndex] =
            ClonePoint(
                candidate,
                solutionCloner,
                isNew: true);

        return true;
    }

    private bool MeetsQualityDistanceAfterReplacement(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        int replacedIndex,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        if (MinimumQualityDistance == 0.0 ||
            QualityTierSize == 1)
        {
            return true;
        }

        double minimum = double.PositiveInfinity;

        for (int i = 0; i < QualityTierSize; i++)
        {
            if (i == replacedIndex)
                continue;

            double value =
                CheckedDistance(
                    referenceSet[i].Solution,
                    candidate,
                    distance);

            if (value < minimum)
                minimum = value;
        }

        return minimum >= MinimumQualityDistance;
    }

    private bool IsDuplicate(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        foreach (ScatterSearchReferencePoint<TSolution> member in referenceSet)
        {
            if (CheckedDistance(
                    member.Solution,
                    candidate,
                    distance) <= DuplicateDistanceTolerance)
            {
                return true;
            }
        }

        return false;
    }

    private static double MinimumDistance(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        double minimum = double.PositiveInfinity;

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

    private static double MinimumDistanceExcluding(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        int excludedIndex,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        double minimum = double.PositiveInfinity;

        for (int i = 0; i < referenceSet.Count; i++)
        {
            if (i == excludedIndex)
                continue;

            double value =
                CheckedDistance(
                    referenceSet[i].Solution,
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

    private static ScatterSearchReferencePoint<TSolution> ClonePoint(
        ScatterSearchReferencePoint<TSolution> point,
        ISolutionCloner<TSolution> solutionCloner,
        bool isNew) =>
        new(
            solutionCloner.Clone(point.Solution),
            point.Objective,
            isNew);

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

/// <summary>
/// Partial RefSet rebuilding that preserves the quality tier and refills the
/// diversity tier from a fresh diversified population by sequential max-min
/// selection.
/// </summary>
public sealed class MaxMinScatterSearchReferenceSetRebuildingMethod<TSolution> :
    IScatterSearchReferenceSetRebuildingMethod<TSolution>
{
    public MaxMinScatterSearchReferenceSetRebuildingMethod(
        double duplicateDistanceTolerance = 0.0)
    {
        if (!double.IsFinite(duplicateDistanceTolerance) ||
            duplicateDistanceTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duplicateDistanceTolerance));
        }

        DuplicateDistanceTolerance = duplicateDistanceTolerance;
    }

    public double DuplicateDistanceTolerance { get; }

    public bool TryRebuild(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> diversifiedPopulation,
        int qualityReferenceSetSize,
        IScatterSearchDistance<TSolution> distance,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner)
    {
        ArgumentNullException.ThrowIfNull(referenceSet);
        ArgumentNullException.ThrowIfNull(diversifiedPopulation);
        ArgumentNullException.ThrowIfNull(distance);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        if (qualityReferenceSetSize <= 0 ||
            qualityReferenceSetSize >= referenceSet.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(qualityReferenceSetSize));
        }

        int targetSize = referenceSet.Count;

        var rebuilt =
            new List<ScatterSearchReferencePoint<TSolution>>(targetSize);

        var qualityTier =
            referenceSet
                .OrderBy(
                    point => point,
                    Comparer<ScatterSearchReferencePoint<TSolution>>.Create(
                        (left, right) =>
                        {
                            if (sense.IsBetter(
                                    left.Objective,
                                    right.Objective))
                            {
                                return -1;
                            }

                            if (sense.IsBetter(
                                    right.Objective,
                                    left.Objective))
                            {
                                return 1;
                            }

                            return 0;
                        }))
                .Take(qualityReferenceSetSize)
                .ToArray();

        foreach (ScatterSearchReferencePoint<TSolution> member in qualityTier)
        {
            rebuilt.Add(
                new ScatterSearchReferencePoint<TSolution>(
                    solutionCloner.Clone(member.Solution),
                    member.Objective,
                    isNew: false));
        }

        var remaining =
            new List<ScatterSearchReferencePoint<TSolution>>(
                diversifiedPopulation);

        while (rebuilt.Count < targetSize)
        {
            ScatterSearchReferencePoint<TSolution>? selected = null;
            double selectedDistance = double.NegativeInfinity;

            foreach (ScatterSearchReferencePoint<TSolution> candidate in remaining)
            {
                if (IsDuplicate(
                        rebuilt,
                        candidate.Solution,
                        distance))
                {
                    continue;
                }

                double minimumDistance =
                    MinimumDistance(
                        rebuilt,
                        candidate.Solution,
                        distance);

                if (minimumDistance > selectedDistance)
                {
                    selectedDistance = minimumDistance;
                    selected = candidate;
                }
                else if (minimumDistance == selectedDistance &&
                         selected is not null &&
                         sense.IsBetter(
                             candidate.Objective,
                             selected.Objective))
                {
                    selected = candidate;
                }
            }

            if (selected is null)
                return false;

            rebuilt.Add(
                new ScatterSearchReferencePoint<TSolution>(
                    solutionCloner.Clone(selected.Solution),
                    selected.Objective,
                    isNew: true));

            remaining.Remove(selected);
        }

        referenceSet.Clear();

        foreach (ScatterSearchReferencePoint<TSolution> point in rebuilt)
            referenceSet.Add(point);

        return true;
    }

    private bool IsDuplicate(
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        foreach (ScatterSearchReferencePoint<TSolution> member in referenceSet)
        {
            if (CheckedDistance(
                    member.Solution,
                    candidate,
                    distance) <= DuplicateDistanceTolerance)
            {
                return true;
            }
        }

        return false;
    }

    private static double MinimumDistance(
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        TSolution candidate,
        IScatterSearchDistance<TSolution> distance)
    {
        double minimum = double.PositiveInfinity;

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
}

/// <summary>
/// Representative Glover/Marti/Laguna subset families:
/// Type 1 pairs, Type 2 augmented triples, Type 3 augmented quadruples and
/// Type 4 nested best-i subsets for i = 5..b.
/// Only subsets containing at least one new RefSet member are returned.
/// </summary>
public sealed class GloverScatterSearchSubsetGenerationMethod<TSolution> :
    IScatterSearchSubsetGenerationMethod<TSolution>
{
    private readonly OptimizationSense _sense;

    public GloverScatterSearchSubsetGenerationMethod(
        OptimizationSense sense) =>
        _sense = sense;

    public IReadOnlyList<ScatterSearchSubset<TSolution>> Generate(
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> referenceSet)
    {
        ArgumentNullException.ThrowIfNull(referenceSet);

        if (referenceSet.Count < 2)
            return Array.Empty<ScatterSearchSubset<TSolution>>();

        var ranked =
            referenceSet
                .Select(
                    (point, originalIndex) =>
                        new RankedPoint(point, originalIndex))
                .OrderBy(
                    item => item,
                    Comparer<RankedPoint>.Create(
                        (left, right) =>
                            CompareObjectives(
                                left.Point.Objective,
                                right.Point.Objective,
                                _sense)))
                .ToArray();

        var result =
            new List<ScatterSearchSubset<TSolution>>();

        var emitted =
            new HashSet<string>(StringComparer.Ordinal);

        var type1 =
            new List<int[]>();

        for (int i = 0; i < ranked.Length; i++)
        {
            for (int j = i + 1; j < ranked.Length; j++)
            {
                int[] subset = new[] { i, j };
                type1.Add(subset);
                AddIfAdmissible(
                    subset,
                    ranked,
                    emitted,
                    result);
            }
        }

        var type2 =
            new List<int[]>();

        foreach (int[] pair in type1)
        {
            int bestOutside =
                FindBestOutside(
                    ranked.Length,
                    pair);

            if (bestOutside < 0)
                continue;

            int[] triple =
                pair
                    .Append(bestOutside)
                    .OrderBy(static value => value)
                    .ToArray();

            if (AddUniqueRaw(type2, triple))
            {
                AddIfAdmissible(
                    triple,
                    ranked,
                    emitted,
                    result);
            }
        }

        var type3 =
            new List<int[]>();

        foreach (int[] triple in type2)
        {
            int bestOutside =
                FindBestOutside(
                    ranked.Length,
                    triple);

            if (bestOutside < 0)
                continue;

            int[] quadruple =
                triple
                    .Append(bestOutside)
                    .OrderBy(static value => value)
                    .ToArray();

            if (AddUniqueRaw(type3, quadruple))
            {
                AddIfAdmissible(
                    quadruple,
                    ranked,
                    emitted,
                    result);
            }
        }

        for (int size = 5; size <= ranked.Length; size++)
        {
            int[] bestSubset =
                Enumerable
                    .Range(0, size)
                    .ToArray();

            AddIfAdmissible(
                bestSubset,
                ranked,
                emitted,
                result);
        }

        return result;
    }

    private static bool AddUniqueRaw(
        IList<int[]> target,
        int[] subset)
    {
        string key =
            string.Join(
                ",",
                subset);

        foreach (int[] existing in target)
        {
            if (string.Equals(
                    string.Join(",", existing),
                    key,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        target.Add(subset);
        return true;
    }

    private static void AddIfAdmissible(
        int[] rankedIndices,
        IReadOnlyList<RankedPoint> ranked,
        ISet<string> emitted,
        ICollection<ScatterSearchSubset<TSolution>> result)
    {
        var members =
            rankedIndices
                .Select(index => ranked[index].Point)
                .ToArray();

        if (!members.Any(static point => point.IsNew))
            return;

        string key =
            string.Join(
                ",",
                rankedIndices.OrderBy(static value => value));

        if (!emitted.Add(key))
            return;

        result.Add(
            new ScatterSearchSubset<TSolution>(
                members));
    }

    private static int FindBestOutside(
        int count,
        IReadOnlyCollection<int> current)
    {
        for (int i = 0; i < count; i++)
        {
            if (!current.Contains(i))
                return i;
        }

        return -1;
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

    private sealed record RankedPoint(
        ScatterSearchReferencePoint<TSolution> Point,
        int OriginalIndex);
}
