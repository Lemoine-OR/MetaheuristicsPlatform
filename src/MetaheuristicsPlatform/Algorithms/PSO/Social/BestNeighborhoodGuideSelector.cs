using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Selects the best personal-best position among topology-defined informers.
/// </summary>
public static class BestNeighborhoodGuideSelector
{
    /// <summary>Returns the best informer index for a particle.</summary>
    public static int Select(
        int particle,
        PsoSocialContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ReadOnlySpan<int> neighbors =
            context.Graph.GetNeighbors(particle);

        if (neighbors.IsEmpty)
        {
            throw new InvalidOperationException(
                $"Particle {particle} has no informers.");
        }

        int best = neighbors[0];
        double bestFitness =
            context.GetPersonalBestFitness(best);

        for (int i = 1; i < neighbors.Length; i++)
        {
            int candidate = neighbors[i];
            double candidateFitness =
                context.GetPersonalBestFitness(candidate);

            if (context.Sense.IsBetter(
                candidateFitness,
                bestFitness))
            {
                best = candidate;
                bestFitness = candidateFitness;
            }
        }

        return best;
    }
}