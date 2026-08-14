namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>Assigns the same relative weight to every informer.</summary>
public sealed class EqualInformerWeightProvider : IInformerWeightProvider
{
    private EqualInformerWeightProvider()
    {
    }

    public static EqualInformerWeightProvider Instance { get; } = new();

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

        weights.Fill(1.0);
    }
}