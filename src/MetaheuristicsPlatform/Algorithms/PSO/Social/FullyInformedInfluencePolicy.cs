using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Fully Informed Particle Swarm (FIPS) attraction structure.
/// Every topology-defined informer contributes its personal-best position.
/// </summary>
/// <remarks>
/// Reference:
/// R. Mendes, J. Kennedy, J. Neves,
/// "The Fully Informed Particle Swarm: Simpler, Maybe Better",
/// IEEE Transactions on Evolutionary Computation 8(3), 204-210, 2004.
/// DOI: 10.1109/TEVC.2004.826074.
///
/// The total acceleration coefficient is distributed uniformly among the
/// particle's informers. Independent random multipliers are sampled per
/// informer and dimension.
/// </remarks>
public sealed class FullyInformedInfluencePolicy : IPsoInfluencePolicy
{
    public FullyInformedInfluencePolicy(
        double totalAccelerationCoefficient)
    {
        if (!double.IsFinite(totalAccelerationCoefficient) ||
            totalAccelerationCoefficient < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalAccelerationCoefficient));
        }

        TotalAccelerationCoefficient =
            totalAccelerationCoefficient;
    }

    public double TotalAccelerationCoefficient { get; }

    public PsoInfluenceDescriptor Descriptor { get; } = new()
    {
        Id = "fips-equal-informer",
        Name = "Fully Informed Particle Swarm (FIPS)",
        UsesOwnPersonalBest = false,
        UsesSingleNeighborhoodGuide = false,
        UsesAllInformers = true,
        IsPublishedExactStructure = true,
        Notes = "Equal coefficient allocation across topology-defined informers.",
        References = new[]
        {
            PsoSocialReferences.MendesKennedyNeves2004
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

        destination.Clear();

        double coefficientPerInformer =
            TotalAccelerationCoefficient / informers.Length;

        ReadOnlySpan<double> current =
            context.GetPosition(particle);

        foreach (int informer in informers)
        {
            ReadOnlySpan<double> informerBest =
                context.GetPersonalBestPosition(informer);

            for (int d = 0; d < destination.Length; d++)
            {
                destination[d] +=
                    coefficientPerInformer *
                    random.NextDouble() *
                    (informerBest[d] - current[d]);
            }
        }
    }
}