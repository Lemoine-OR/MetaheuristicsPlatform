using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.State;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>
/// Fused particle movement kernels for built-in PSO influence policies.
/// </summary>
internal static class PsoMovementKernel
{
    internal static void UpdateRange(
        int start,
        int end,
        PsoSwarmBuffers buffers,
        PsoSocialContext socialContext,
        IPsoInfluencePolicy influencePolicy,
        PsoVelocityCoefficients dynamics,
        PsoParticleRandomStreams randomStreams,
        int[] neighborhoodBestGuides,
        ReadOnlySpan<double> lowerBounds,
        ReadOnlySpan<double> upperBounds,
        ReadOnlySpan<double> velocityLimits,
        PsoBoundaryHandling boundaryHandling,
        double[] attractionScratch)
    {
        for (int particle = start;
             particle < end;
             particle++)
        {
            if (influencePolicy is
                CanonicalBestInfluencePolicy canonical)
            {
                UpdateCanonical(
                    particle,
                    buffers,
                    socialContext,
                    canonical,
                    dynamics,
                    randomStreams.Get(particle),
                    neighborhoodBestGuides[particle],
                    lowerBounds,
                    upperBounds,
                    velocityLimits,
                    boundaryHandling);

                continue;
            }

            if (influencePolicy is
                FullyInformedInfluencePolicy fips)
            {
                UpdateFips(
                    particle,
                    buffers,
                    socialContext,
                    fips,
                    dynamics,
                    randomStreams.Get(particle),
                    lowerBounds,
                    upperBounds,
                    velocityLimits,
                    boundaryHandling);

                continue;
            }

            Span<double> attraction =
                attractionScratch.AsSpan(
                    particle * buffers.Dimension,
                    buffers.Dimension);

            influencePolicy.ComputeAttraction(
                particle,
                socialContext,
                randomStreams.Get(particle),
                attraction);

            ApplyAttraction(
                particle,
                buffers,
                attraction,
                dynamics,
                lowerBounds,
                upperBounds,
                velocityLimits,
                boundaryHandling);
        }
    }

    private static void UpdateCanonical(
        int particle,
        PsoSwarmBuffers buffers,
        PsoSocialContext context,
        CanonicalBestInfluencePolicy policy,
        PsoVelocityCoefficients dynamics,
        IRandomSource random,
        int guide,
        ReadOnlySpan<double> lowerBounds,
        ReadOnlySpan<double> upperBounds,
        ReadOnlySpan<double> velocityLimits,
        PsoBoundaryHandling boundaryHandling)
    {
        Span<double> position =
            buffers.GetPosition(particle);

        Span<double> velocity =
            buffers.GetVelocity(particle);

        ReadOnlySpan<double> personalBest =
            context.GetPersonalBestPosition(
                particle);

        ReadOnlySpan<double> guideBest =
            context.GetPersonalBestPosition(
                guide);

        for (int d = 0;
             d < position.Length;
             d++)
        {
            double attraction =
                policy.CognitiveCoefficient *
                    random.NextDouble() *
                    (personalBest[d] -
                     position[d]) +
                policy.SocialCoefficient *
                    random.NextDouble() *
                    (guideBest[d] -
                     position[d]);

            double nextVelocity =
                dynamics.PreviousVelocityMultiplier *
                    velocity[d] +
                dynamics.AttractionMultiplier *
                    attraction;

            nextVelocity =
                LimitVelocity(
                    nextVelocity,
                    velocityLimits,
                    d);

            double nextPosition =
                position[d] +
                nextVelocity;

            ApplyBoundary(
                ref nextPosition,
                ref nextVelocity,
                lowerBounds[d],
                upperBounds[d],
                boundaryHandling);

            velocity[d] = nextVelocity;
            position[d] = nextPosition;
        }
    }

    private static void UpdateFips(
        int particle,
        PsoSwarmBuffers buffers,
        PsoSocialContext context,
        FullyInformedInfluencePolicy policy,
        PsoVelocityCoefficients dynamics,
        IRandomSource random,
        ReadOnlySpan<double> lowerBounds,
        ReadOnlySpan<double> upperBounds,
        ReadOnlySpan<double> velocityLimits,
        PsoBoundaryHandling boundaryHandling)
    {
        Span<double> position =
            buffers.GetPosition(particle);

        Span<double> velocity =
            buffers.GetVelocity(particle);

        ReadOnlySpan<int> informers =
            context.Graph.GetNeighbors(particle);

        if (informers.IsEmpty)
        {
            throw new InvalidOperationException(
                $"Particle {particle} has no informers.");
        }

        double coefficient =
            policy.TotalAccelerationCoefficient /
            informers.Length;

        for (int d = 0;
             d < position.Length;
             d++)
        {
            double current =
                position[d];

            double attraction = 0.0;

            foreach (int informer in informers)
            {
                ReadOnlySpan<double> informerBest =
                    context.GetPersonalBestPosition(
                        informer);

                attraction +=
                    coefficient *
                    random.NextDouble() *
                    (informerBest[d] -
                     current);
            }

            double nextVelocity =
                dynamics.PreviousVelocityMultiplier *
                    velocity[d] +
                dynamics.AttractionMultiplier *
                    attraction;

            nextVelocity =
                LimitVelocity(
                    nextVelocity,
                    velocityLimits,
                    d);

            double nextPosition =
                current +
                nextVelocity;

            ApplyBoundary(
                ref nextPosition,
                ref nextVelocity,
                lowerBounds[d],
                upperBounds[d],
                boundaryHandling);

            velocity[d] = nextVelocity;
            position[d] = nextPosition;
        }
    }

    private static void ApplyAttraction(
        int particle,
        PsoSwarmBuffers buffers,
        ReadOnlySpan<double> attraction,
        PsoVelocityCoefficients dynamics,
        ReadOnlySpan<double> lowerBounds,
        ReadOnlySpan<double> upperBounds,
        ReadOnlySpan<double> velocityLimits,
        PsoBoundaryHandling boundaryHandling)
    {
        Span<double> position =
            buffers.GetPosition(particle);

        Span<double> velocity =
            buffers.GetVelocity(particle);

        for (int d = 0;
             d < position.Length;
             d++)
        {
            double nextVelocity =
                dynamics.PreviousVelocityMultiplier *
                    velocity[d] +
                dynamics.AttractionMultiplier *
                    attraction[d];

            nextVelocity =
                LimitVelocity(
                    nextVelocity,
                    velocityLimits,
                    d);

            double nextPosition =
                position[d] +
                nextVelocity;

            ApplyBoundary(
                ref nextPosition,
                ref nextVelocity,
                lowerBounds[d],
                upperBounds[d],
                boundaryHandling);

            velocity[d] = nextVelocity;
            position[d] = nextPosition;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double LimitVelocity(
        double velocity,
        ReadOnlySpan<double> limits,
        int dimension)
    {
        if (limits.IsEmpty)
        {
            return velocity;
        }

        double limit = limits[dimension];

        return Math.Clamp(
            velocity,
            -limit,
            limit);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyBoundary(
        ref double position,
        ref double velocity,
        double lower,
        double upper,
        PsoBoundaryHandling policy)
    {
        if (position >= lower &&
            position <= upper)
        {
            return;
        }

        switch (policy)
        {
            case PsoBoundaryHandling.None:
                return;

            case PsoBoundaryHandling.Clamp:
                position =
                    Math.Clamp(
                        position,
                        lower,
                        upper);
                return;

            case PsoBoundaryHandling.ClampAndZeroVelocity:
                position =
                    Math.Clamp(
                        position,
                        lower,
                        upper);
                velocity = 0.0;
                return;

            case PsoBoundaryHandling.Reflect:
                Reflect(
                    ref position,
                    ref velocity,
                    lower,
                    upper);
                return;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(policy));
        }
    }

    private static void Reflect(
        ref double position,
        ref double velocity,
        double lower,
        double upper)
    {
        double width =
            upper - lower;

        if (!(width > 0.0))
        {
            throw new InvalidOperationException(
                "Search-space width must be strictly positive.");
        }

        double period =
            2.0 * width;

        double shifted =
            (position - lower) %
            period;

        if (shifted < 0.0)
        {
            shifted += period;
        }

        if (shifted <= width)
        {
            position =
                lower + shifted;
        }
        else
        {
            position =
                upper -
                (shifted - width);
        }

        // Reflection changes the direction of travel at the boundary.
        velocity = -velocity;
    }
}