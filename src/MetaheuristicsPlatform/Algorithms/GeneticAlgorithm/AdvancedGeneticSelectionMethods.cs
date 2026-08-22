using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Uniformly selects one parent from the best configured fraction of the population.
/// </summary>
public sealed class TruncationGeneticParentSelectionMethod<TSolution> :
    IGeneticParentSelectionMethod<TSolution>
{
    public TruncationGeneticParentSelectionMethod(
        double selectedFraction = 0.5)
    {
        if (!double.IsFinite(selectedFraction) ||
            selectedFraction <= 0.0 ||
            selectedFraction > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedFraction));
        }

        SelectedFraction = selectedFraction;
    }

    public double SelectedFraction { get; }

    public int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random)
    {
        GeneticSelectionUtilities.ValidatePopulation(population, random);

        int[] ranked =
            GeneticSelectionUtilities.RankBestFirst(
                population,
                sense);

        int eligible =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    population.Count * SelectedFraction));

        return ranked[random.NextInt32(eligible)];
    }
}

/// <summary>
/// Linear rank selection with selective pressure in [1,2].
/// Rank zero is best and receives probability s/N.
/// </summary>
public sealed class LinearRankingGeneticParentSelectionMethod<TSolution> :
    IGeneticParentSelectionMethod<TSolution>
{
    public LinearRankingGeneticParentSelectionMethod(
        double selectivePressure = 1.5)
    {
        if (!double.IsFinite(selectivePressure) ||
            selectivePressure < 1.0 ||
            selectivePressure > 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(selectivePressure));
        }

        SelectivePressure = selectivePressure;
    }

    public double SelectivePressure { get; }

    public int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random)
    {
        GeneticSelectionUtilities.ValidatePopulation(population, random);

        if (population.Count == 1)
            return 0;

        int[] ranked =
            GeneticSelectionUtilities.RankBestFirst(
                population,
                sense);

        double[] weights =
            new double[population.Count];

        for (int rank = 0;
             rank < weights.Length;
             rank++)
        {
            weights[rank] =
                SelectivePressure -
                (2.0 * (SelectivePressure - 1.0) * rank /
                 (weights.Length - 1));
        }

        int rankedIndex =
            GeneticSelectionUtilities.SampleWeights(
                weights,
                random);

        return ranked[rankedIndex];
    }
}

/// <summary>
/// Exponential rank selection with weights exp(-decay * rank), rank zero being best.
/// </summary>
public sealed class ExponentialRankingGeneticParentSelectionMethod<TSolution> :
    IGeneticParentSelectionMethod<TSolution>
{
    public ExponentialRankingGeneticParentSelectionMethod(
        double decay = 0.25)
    {
        if (!double.IsFinite(decay) || decay <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(decay));

        Decay = decay;
    }

    public double Decay { get; }

    public int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random)
    {
        GeneticSelectionUtilities.ValidatePopulation(population, random);

        int[] ranked =
            GeneticSelectionUtilities.RankBestFirst(
                population,
                sense);

        double[] weights =
            new double[population.Count];

        for (int rank = 0;
             rank < weights.Length;
             rank++)
        {
            weights[rank] =
                Math.Exp(-Decay * rank);
        }

        int rankedIndex =
            GeneticSelectionUtilities.SampleWeights(
                weights,
                random);

        return ranked[rankedIndex];
    }
}

/// <summary>
/// Fitness-proportionate parent selection using explicit user-supplied non-negative weights.
/// Objective values are never silently assumed to already be valid roulette-wheel fitnesses.
/// </summary>
public sealed class ExplicitFitnessProportionateGeneticParentSelectionMethod<TSolution> :
    IGeneticParentSelectionMethod<TSolution>
{
    private readonly Func<GeneticPopulationMember<TSolution>,OptimizationSense,double> _weightSelector;

    public ExplicitFitnessProportionateGeneticParentSelectionMethod(
        Func<GeneticPopulationMember<TSolution>,OptimizationSense,double> weightSelector)
    {
        _weightSelector =
            weightSelector ??
            throw new ArgumentNullException(nameof(weightSelector));
    }

    public int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random)
    {
        GeneticSelectionUtilities.ValidatePopulation(population, random);

        double[] weights =
            new double[population.Count];

        for (int index = 0;
             index < population.Count;
             index++)
        {
            double weight =
                _weightSelector(
                    population[index],
                    sense);

            if (!double.IsFinite(weight) || weight < 0.0)
            {
                throw new InvalidOperationException(
                    "Fitness-proportionate selection requires finite non-negative explicit weights.");
            }

            weights[index] = weight;
        }

        return GeneticSelectionUtilities.SampleWeights(
            weights,
            random);
    }
}

internal static class GeneticSelectionUtilities
{
    public static void ValidatePopulation<TSolution>(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);

        if (population.Count == 0)
        {
            throw new ArgumentException(
                "Parent selection requires a non-empty population.",
                nameof(population));
        }
    }

    public static int[] RankBestFirst<TSolution>(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense)
    {
        int[] indices =
            Enumerable
                .Range(0, population.Count)
                .ToArray();

        Array.Sort(
            indices,
            (left, right) =>
            {
                double leftObjective =
                    population[left].Objective;
                double rightObjective =
                    population[right].Objective;

                if (sense.IsBetter(leftObjective, rightObjective))
                    return -1;

                if (sense.IsBetter(rightObjective, leftObjective))
                    return 1;

                return left.CompareTo(right);
            });

        return indices;
    }

    public static int SampleWeights(
        IReadOnlyList<double> weights,
        IRandomSource random)
    {
        double total = 0.0;

        for (int index = 0;
             index < weights.Count;
             index++)
        {
            double weight = weights[index];

            if (!double.IsFinite(weight) || weight < 0.0)
            {
                throw new InvalidOperationException(
                    "Selection weights must be finite and non-negative.");
            }

            total += weight;
        }

        if (!double.IsFinite(total) || total <= 0.0)
        {
            throw new InvalidOperationException(
                "At least one strictly positive selection weight is required.");
        }

        double threshold =
            random.NextDouble() * total;

        double cumulative = 0.0;
        int lastPositive = -1;

        for (int index = 0;
             index < weights.Count;
             index++)
        {
            double weight = weights[index];

            if (weight > 0.0)
                lastPositive = index;

            cumulative += weight;

            if (threshold < cumulative)
                return index;
        }

        if (lastPositive >= 0)
            return lastPositive;

        throw new InvalidOperationException(
            "Weighted selection failed despite a positive total weight.");
    }
}
