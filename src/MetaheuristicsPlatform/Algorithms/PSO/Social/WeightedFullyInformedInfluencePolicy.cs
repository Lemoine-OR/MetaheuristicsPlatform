using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Generic weighted fully-informed attraction.
/// </summary>
/// <remarks>
/// This reusable mechanism is intentionally not named SFIPSO.
/// Exact SFIPSO requires additional population-state and topology-construction
/// mechanisms from Zhang &amp; Yi (2011).
/// </remarks>
public sealed class WeightedFullyInformedInfluencePolicy : IPsoInfluencePolicy
{
    private readonly IInformerWeightProvider _weightProvider;

    public WeightedFullyInformedInfluencePolicy(
        double totalAccelerationCoefficient,
        IInformerWeightProvider weightProvider)
    {
        if (!double.IsFinite(totalAccelerationCoefficient) ||
            totalAccelerationCoefficient < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalAccelerationCoefficient));
        }

        TotalAccelerationCoefficient =
            totalAccelerationCoefficient;

        _weightProvider = weightProvider ??
            throw new ArgumentNullException(
                nameof(weightProvider));
    }

    public double TotalAccelerationCoefficient { get; }

    public PsoInfluenceDescriptor Descriptor { get; } = new()
    {
        Id = "weighted-fully-informed-generic",
        Name = "Weighted Fully Informed (Generic)",
        UsesOwnPersonalBest = false,
        UsesSingleNeighborhoodGuide = false,
        UsesAllInformers = true,
        IsPublishedExactStructure = false,
        Notes = "Reusable weighted informer mechanism. Not the exact SFIPSO algorithm.",
        References = new[]
        {
            PsoSocialReferences.MendesKennedyNeves2004,
            PsoSocialReferences.ZhangYi2011
        }
    };

    public void ComputeAttraction(
        int particle,
        PsoSocialContext context,
        IRandomSource random,
        Span<double> destination)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        if (destination.Length != context.Dimension)
        {
            throw new ArgumentException(
                $"Expected destination dimension {context.Dimension}, " +
                $"received {destination.Length}.",
                nameof(destination));
        }

        ReadOnlySpan<int> informers =
            context.Graph.GetNeighbors(particle);

        if (informers.IsEmpty)
        {
            throw new InvalidOperationException(
                $"Particle {particle} has no informers.");
        }

        Span<double> weights =
            informers.Length <= 128
                ? stackalloc double[informers.Length]
                : new double[informers.Length];

        _weightProvider.GetWeights(
            particle,
            context,
            informers,
            weights);

        double totalWeight = 0.0;

        for (int i = 0; i < weights.Length; i++)
        {
            double weight = weights[i];

            if (!double.IsFinite(weight) ||
                weight < 0.0)
            {
                throw new InvalidOperationException(
                    "Informer weights must be finite and non-negative.");
            }

            totalWeight += weight;
        }

        if (!(totalWeight > 0.0) ||
            !double.IsFinite(totalWeight))
        {
            throw new InvalidOperationException(
                "At least one strictly positive finite informer weight is required.");
        }

        destination.Clear();

        ReadOnlySpan<double> current =
            context.GetPosition(particle);

        for (int informerIndex = 0;
             informerIndex < informers.Length;
             informerIndex++)
        {
            double normalizedCoefficient =
                TotalAccelerationCoefficient *
                (weights[informerIndex] / totalWeight);

            if (normalizedCoefficient == 0.0)
            {
                continue;
            }

            ReadOnlySpan<double> informerBest =
                context.GetPersonalBestPosition(
                    informers[informerIndex]);

            for (int d = 0;
                 d < destination.Length;
                 d++)
            {
                destination[d] +=
                    normalizedCoefficient *
                    random.NextDouble() *
                    (informerBest[d] - current[d]);
            }
        }
    }
}