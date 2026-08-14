using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Classical stochastic cognitive + best-neighborhood attraction.
/// </summary>
public sealed class CanonicalBestInfluencePolicy : IPsoInfluencePolicy
{
    public CanonicalBestInfluencePolicy(
        double cognitiveCoefficient,
        double socialCoefficient)
    {
        if (!double.IsFinite(cognitiveCoefficient) ||
            cognitiveCoefficient < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cognitiveCoefficient));
        }

        if (!double.IsFinite(socialCoefficient) ||
            socialCoefficient < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(socialCoefficient));
        }

        CognitiveCoefficient = cognitiveCoefficient;
        SocialCoefficient = socialCoefficient;
    }

    public double CognitiveCoefficient { get; }
    public double SocialCoefficient { get; }

    public PsoInfluenceDescriptor Descriptor { get; } = new()
    {
        Id = "canonical-best-neighborhood",
        Name = "Canonical Cognitive + Best-Neighborhood Influence",
        UsesOwnPersonalBest = true,
        UsesSingleNeighborhoodGuide = true,
        UsesAllInformers = false,
        IsPublishedExactStructure = true,
        References = new[]
        {
            PsoSocialReferences.KennedyEberhart1995,
            PsoSocialReferences.ClercKennedy2002
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

        ReadOnlySpan<double> current =
            context.GetPosition(particle);

        ReadOnlySpan<double> personalBest =
            context.GetPersonalBestPosition(particle);

        int guide =
            BestNeighborhoodGuideSelector.Select(
                particle,
                context);

        ReadOnlySpan<double> guideBest =
            context.GetPersonalBestPosition(guide);

        for (int d = 0; d < destination.Length; d++)
        {
            double cognitive =
                CognitiveCoefficient *
                random.NextDouble() *
                (personalBest[d] - current[d]);

            double social =
                SocialCoefficient *
                random.NextDouble() *
                (guideBest[d] - current[d]);

            destination[d] = cognitive + social;
        }
    }
}