namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Uses caller-supplied per-particle informer weights.
/// Intended for experiments and custom algorithms.
/// </summary>
public sealed class FixedInformerWeightProvider : IInformerWeightProvider
{
    private readonly Func<int, int, double> _weight;

    public FixedInformerWeightProvider(
        Func<int, int, double> weight)
    {
        _weight = weight ??
            throw new ArgumentNullException(nameof(weight));
    }

    public void GetWeights(
        int particle,
        PsoSocialContext context,
        ReadOnlySpan<int> informers,
        Span<double> weights)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (weights.Length != informers.Length)
        {
            throw new ArgumentException(
                "Weight buffer length must equal informer count.",
                nameof(weights));
        }

        for (int i = 0; i < informers.Length; i++)
        {
            double value = _weight(
                particle,
                informers[i]);

            if (!double.IsFinite(value) ||
                value < 0.0)
            {
                throw new InvalidOperationException(
                    "Informer weights must be finite and non-negative.");
            }

            weights[i] = value;
        }
    }
}