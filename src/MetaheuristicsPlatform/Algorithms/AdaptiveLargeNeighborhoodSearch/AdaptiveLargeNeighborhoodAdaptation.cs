using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public static class AdaptiveLargeNeighborhoodAdaptation
{
    public static int SelectIndex(IReadOnlyList<double> weights, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(weights);
        ArgumentNullException.ThrowIfNull(random);

        if (weights.Count == 0)
            throw new ArgumentException("Adaptive LNS operator pool must be non-empty.", nameof(weights));

        double total = 0.0;
        for (int i = 0; i < weights.Count; i++)
        {
            double weight = weights[i];
            if (!double.IsFinite(weight) || weight < 0.0)
                throw new ArgumentOutOfRangeException(nameof(weights));
            total += weight;
        }

        if (!double.IsFinite(total))
            throw new InvalidOperationException("Adaptive LNS operator-weight sum is non-finite.");

        if (total <= 0.0)
            return random.NextInt32(weights.Count);

        double target = random.NextDouble() * total;
        double cumulative = 0.0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += weights[i];
            if (target < cumulative)
                return i;
        }

        return weights.Count - 1;
    }

    public static double UpdateWeight(
        double currentWeight,
        double accumulatedScore,
        int usageCount,
        double reactionFactor)
    {
        if (!double.IsFinite(currentWeight) || currentWeight < 0.0)
            throw new ArgumentOutOfRangeException(nameof(currentWeight));
        if (!double.IsFinite(accumulatedScore) || accumulatedScore < 0.0)
            throw new ArgumentOutOfRangeException(nameof(accumulatedScore));
        if (usageCount < 0)
            throw new ArgumentOutOfRangeException(nameof(usageCount));
        if (!double.IsFinite(reactionFactor) || reactionFactor < 0.0 || reactionFactor > 1.0)
            throw new ArgumentOutOfRangeException(nameof(reactionFactor));

        if (usageCount == 0)
            return currentWeight;

        double averageScore = accumulatedScore / usageCount;

        return ((1.0 - reactionFactor) * currentWeight) +
               (reactionFactor * averageScore);
    }

    public static double DetermineReward(
        bool isNovel,
        bool isNewGlobalBest,
        bool improvesCurrent,
        bool accepted,
        AdaptiveLargeNeighborhoodSearchParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!isNovel) return 0.0;
        if (isNewGlobalBest) return parameters.GlobalBestReward;
        if (accepted && improvesCurrent) return parameters.ImprovingReward;
        if (accepted) return parameters.AcceptedReward;
        return 0.0;
    }
}
