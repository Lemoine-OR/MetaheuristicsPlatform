using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>Inputs available to a memetic local-search application policy.</summary>
public readonly record struct MemeticLocalSearchCandidateContext(
    int Generation,
    int OffspringIndex,
    int OffspringRank,
    int OffspringCount,
    double Objective,
    double BestObjective,
    int ConsecutiveNonImprovingGenerations,
    long LocalSearchInvocations,
    long SuccessfulLocalSearches);

/// <summary>Decision returned by a memetic local-search application policy.</summary>
public readonly record struct MemeticLocalSearchDecision(
    bool Apply,
    double Probability);

/// <summary>
/// Chooses which newly generated population members receive local improvement.
/// Policies are representation independent and can therefore be reused by later
/// population engines.
/// </summary>
public interface IMemeticLocalSearchPolicy
{
    string Id { get; }

    /// <summary>Whether the policy requires objective ranking of the offspring block.</summary>
    bool RequiresRanking { get; }

    MemeticLocalSearchDecision Decide(
        in MemeticLocalSearchCandidateContext context,
        IRandomSource random);
}

/// <summary>Applies local search to every newly generated offspring.</summary>
public sealed class EveryOffspringMemeticLocalSearchPolicy :
    IMemeticLocalSearchPolicy
{
    public string Id =>
        MemeticAlgorithmComponentIds.EveryOffspring;

    public bool RequiresRanking => false;

    public MemeticLocalSearchDecision Decide(
        in MemeticLocalSearchCandidateContext context,
        IRandomSource random) =>
        new(true, 1.0);
}

/// <summary>
/// Applies local search to all offspring every <c>Period</c> generations.
/// Generation numbering starts at one.
/// </summary>
public sealed class PeriodicMemeticLocalSearchPolicy :
    IMemeticLocalSearchPolicy
{
    public PeriodicMemeticLocalSearchPolicy(
        int period)
    {
        if (period <= 0)
            throw new ArgumentOutOfRangeException(nameof(period));

        Period = period;
    }

    public int Period { get; }

    public string Id =>
        MemeticAlgorithmComponentIds.Periodic;

    public bool RequiresRanking => false;

    public MemeticLocalSearchDecision Decide(
        in MemeticLocalSearchCandidateContext context,
        IRandomSource random)
    {
        bool apply =
            context.Generation % Period == 0;

        return new(
            apply,
            apply ? 1.0 : 0.0);
    }
}

/// <summary>Applies local search independently with a fixed probability.</summary>
public sealed class ProbabilisticMemeticLocalSearchPolicy :
    IMemeticLocalSearchPolicy
{
    public ProbabilisticMemeticLocalSearchPolicy(
        double probability)
    {
        if (!double.IsFinite(probability) ||
            probability < 0.0 ||
            probability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(probability));
        }

        Probability = probability;
    }

    public double Probability { get; }

    public string Id =>
        MemeticAlgorithmComponentIds.Probabilistic;

    public bool RequiresRanking => false;

    public MemeticLocalSearchDecision Decide(
        in MemeticLocalSearchCandidateContext context,
        IRandomSource random)
    {
        if (Probability <= 0.0)
            return new(false, 0.0);

        if (Probability >= 1.0)
            return new(true, 1.0);

        return new(
            random.NextDouble() < Probability,
            Probability);
    }
}

/// <summary>
/// Applies local search to the best objective-ranked fraction of newly generated
/// offspring. Elites copied from the preceding generation are excluded by the engine.
/// </summary>
public sealed class TopFractionMemeticLocalSearchPolicy :
    IMemeticLocalSearchPolicy
{
    public TopFractionMemeticLocalSearchPolicy(
        double fraction)
    {
        if (!double.IsFinite(fraction) ||
            fraction <= 0.0 ||
            fraction > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fraction));
        }

        Fraction = fraction;
    }

    public double Fraction { get; }

    public string Id =>
        MemeticAlgorithmComponentIds.TopFraction;

    public bool RequiresRanking => true;

    public MemeticLocalSearchDecision Decide(
        in MemeticLocalSearchCandidateContext context,
        IRandomSource random)
    {
        int selected =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    context.OffspringCount *
                    Fraction));

        bool apply =
            context.OffspringRank < selected;

        return new(
            apply,
            apply ? 1.0 : 0.0);
    }
}

/// <summary>
/// Linearly increases local-search pressure as the number of consecutive generations
/// without a global improvement approaches <c>StagnationWindow</c>.
/// </summary>
public sealed class StagnationAdaptiveMemeticLocalSearchPolicy :
    IMemeticLocalSearchPolicy
{
    public StagnationAdaptiveMemeticLocalSearchPolicy(
        double minimumProbability = 0.1,
        double maximumProbability = 1.0,
        int stagnationWindow = 10)
    {
        if (!double.IsFinite(minimumProbability) ||
            minimumProbability < 0.0 ||
            minimumProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumProbability));
        }

        if (!double.IsFinite(maximumProbability) ||
            maximumProbability < minimumProbability ||
            maximumProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumProbability));
        }

        if (stagnationWindow <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stagnationWindow));
        }

        MinimumProbability = minimumProbability;
        MaximumProbability = maximumProbability;
        StagnationWindow = stagnationWindow;
    }

    public double MinimumProbability { get; }

    public double MaximumProbability { get; }

    public int StagnationWindow { get; }

    public string Id =>
        MemeticAlgorithmComponentIds.AdaptiveStagnation;

    public bool RequiresRanking => false;

    public MemeticLocalSearchDecision Decide(
        in MemeticLocalSearchCandidateContext context,
        IRandomSource random)
    {
        double pressure =
            Math.Min(
                1.0,
                (double)context.ConsecutiveNonImprovingGenerations /
                StagnationWindow);

        double probability =
            MinimumProbability +
            (MaximumProbability - MinimumProbability) *
            pressure;

        if (probability <= 0.0)
            return new(false, 0.0);

        if (probability >= 1.0)
            return new(true, 1.0);

        return new(
            random.NextDouble() < probability,
            probability);
    }
}
