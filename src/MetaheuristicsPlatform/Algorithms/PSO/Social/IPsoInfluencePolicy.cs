using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Computes the stochastic attraction contribution for one particle.
/// The caller owns and reuses the destination buffer.
/// </summary>
public interface IPsoInfluencePolicy
{
    PsoInfluenceDescriptor Descriptor { get; }

    /// <summary>
    /// Overwrites <paramref name="destination"/> with the attraction vector for
    /// <paramref name="particle"/>.
    /// </summary>
    void ComputeAttraction(
        int particle,
        PsoSocialContext context,
        IRandomSource random,
        Span<double> destination);
}