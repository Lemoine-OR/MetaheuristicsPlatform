using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Precomputes the best informer used by canonical PSO.
/// </summary>
internal static class PsoNeighborhoodGuideCache
{
    internal static void Fill(
        int[] guides,
        PsoSocialContext context,
        IPsoTopology topology,
        PsoExecutionOptions execution,
        CancellationToken cancellationToken)
    {
        if (guides.Length != context.SwarmSize)
        {
            throw new ArgumentException(
                "Guide buffer length must equal swarm size.",
                nameof(guides));
        }

        if (topology is FullyConnectedTopology)
        {
            int globalBest = 0;
            double globalBestFitness =
                context.GetPersonalBestFitness(0);

            for (int particle = 1;
                 particle < context.SwarmSize;
                 particle++)
            {
                double fitness =
                    context.GetPersonalBestFitness(
                        particle);

                if (context.Sense.IsBetter(
                    fitness,
                    globalBestFitness))
                {
                    globalBest = particle;
                    globalBestFitness = fitness;
                }
            }

            Array.Fill(
                guides,
                globalBest);

            return;
        }

        PsoRangeExecutor.ForParticles(
            context.SwarmSize,
            context.Dimension,
            execution,
            (start, end) =>
            {
                for (int particle = start;
                     particle < end;
                     particle++)
                {
                    guides[particle] =
                        BestNeighborhoodGuideSelector.Select(
                            particle,
                            context);
                }
            },
            cancellationToken);
    }
}