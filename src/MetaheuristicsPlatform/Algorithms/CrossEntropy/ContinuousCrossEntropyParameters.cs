using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.CrossEntropy;

/// <summary>
/// Parameters of the diagonal-normal continuous Cross-Entropy Method.
/// </summary>
public sealed class ContinuousCrossEntropyParameters : IMetaheuristicParameters
{
    /// <summary>Number of sampled candidates per complete CE iteration.</summary>
    public int SampleCount { get; init; } = 100;

    /// <summary>Fraction of the sampled population retained as elite.</summary>
    public double EliteFraction { get; init; } = 0.10;

    /// <summary>Maximum number of complete CE iterations.</summary>
    public int MaximumIterations { get; init; } = 500;

    /// <summary>
    /// Fixed smoothing coefficient for the elite mean update.
    /// Kroese et al. discuss values between 0.5 and 0.9.
    /// </summary>
    public double MeanSmoothing { get; init; } = 0.70;

    /// <summary>
    /// Base coefficient beta of the dynamic standard-deviation smoothing law.
    /// </summary>
    public double StandardDeviationSmoothingBase { get; init; } = 0.90;

    /// <summary>
    /// Exponent q of beta_t = beta - beta(1 - 1/t)^q.
    /// </summary>
    public double DynamicSmoothingExponent { get; init; } = 5.0;

    /// <summary>
    /// Initial coordinate standard deviation as a fraction of box width.
    /// </summary>
    public double InitialStandardDeviationScale { get; init; } = 0.50;

    /// <summary>
    /// Numerical floor and local distribution-collapse threshold.
    /// </summary>
    public double MinimumStandardDeviation { get; init; } = 1e-8;

    /// <summary>
    /// Optional first mean. Null selects the center of the bounded box.
    /// </summary>
    public double[]? InitialMean { get; init; }

    public void Validate()
    {
        if (SampleCount < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(SampleCount));
        }

        if (!double.IsFinite(EliteFraction) ||
            EliteFraction <= 0.0 ||
            EliteFraction >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(EliteFraction));
        }

        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }

        if (!double.IsFinite(MeanSmoothing) ||
            MeanSmoothing <= 0.0 ||
            MeanSmoothing > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(MeanSmoothing));
        }

        if (!double.IsFinite(StandardDeviationSmoothingBase) ||
            StandardDeviationSmoothingBase <= 0.0 ||
            StandardDeviationSmoothingBase >= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StandardDeviationSmoothingBase));
        }

        if (!double.IsFinite(DynamicSmoothingExponent) ||
            DynamicSmoothingExponent <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(DynamicSmoothingExponent));
        }

        if (!double.IsFinite(InitialStandardDeviationScale) ||
            InitialStandardDeviationScale <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialStandardDeviationScale));
        }

        if (!double.IsFinite(MinimumStandardDeviation) ||
            MinimumStandardDeviation <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumStandardDeviation));
        }

        if (InitialMean is not null)
        {
            for (int i = 0; i < InitialMean.Length; i++)
            {
                if (!double.IsFinite(InitialMean[i]))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(InitialMean),
                        $"InitialMean[{i}] must be finite.");
                }
            }
        }
    }
}
