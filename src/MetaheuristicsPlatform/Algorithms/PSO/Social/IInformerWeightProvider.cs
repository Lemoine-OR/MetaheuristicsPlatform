namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Supplies non-negative relative weights for one particle's topology-defined informers.
/// </summary>
public interface IInformerWeightProvider
{
    /// <summary>
    /// Writes one relative weight per informer into <paramref name="weights"/>.
    /// The provider does not need to normalize them.
    /// </summary>
    void GetWeights(
        int particle,
        PsoSocialContext context,
        ReadOnlySpan<int> informers,
        Span<double> weights);
}