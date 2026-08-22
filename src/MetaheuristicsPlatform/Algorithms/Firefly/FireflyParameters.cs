using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Firefly;

/// <summary>Parameters of the canonical continuous Firefly Algorithm.</summary>
public sealed class FireflyParameters : IMetaheuristicParameters
{
    /// <summary>Number of fireflies in the population.</summary>
    public int PopulationSize { get; init; } = 20;

    /// <summary>Maximum number of complete pairwise-attraction iterations.</summary>
    public int MaximumIterations { get; init; } = 500;

    /// <summary>Attractiveness beta_0 at zero distance.</summary>
    public double BaseAttractiveness { get; init; } = 1.0;

    /// <summary>Light absorption coefficient gamma.</summary>
    public double LightAbsorptionCoefficient { get; init; } = 1.0;

    /// <summary>
    /// Additive randomization amplitude alpha in the original movement equation.
    /// The random term is alpha * (U(0,1) - 1/2) per coordinate.
    /// </summary>
    public double RandomizationAmplitude { get; init; } = 0.2;

    public void Validate()
    {
        if (PopulationSize < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        }

        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }

        if (!double.IsFinite(BaseAttractiveness) ||
            BaseAttractiveness < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(BaseAttractiveness));
        }

        if (!double.IsFinite(LightAbsorptionCoefficient) ||
            LightAbsorptionCoefficient < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(LightAbsorptionCoefficient));
        }

        if (!double.IsFinite(RandomizationAmplitude) ||
            RandomizationAmplitude < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(RandomizationAmplitude));
        }
    }
}
