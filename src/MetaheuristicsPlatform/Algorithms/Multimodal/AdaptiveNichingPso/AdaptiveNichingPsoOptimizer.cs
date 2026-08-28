using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.AdaptiveNichingPso;

public sealed class AdaptiveNichingPsoOptimizer :
    IMultimodalOptimizer<AdaptiveNichingPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.AdaptiveNichingPso,
            Name = "Adaptive Niching Particle Swarm Optimization",
            Acronym = "ANPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    AdaptiveNichingPsoOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    AdaptiveNichingPsoParameters parameters,
    OptimizationOptions? options = null,
    CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(problem);
    ArgumentNullException.ThrowIfNull(parameters);
    parameters.Validate();

    IRandomSource random =
        MultimodalToolkit.CreateRandom(
            options,
            out ulong seed);

    int evaluations = 0;
    List<MultimodalCandidate> swarm =
        MultimodalToolkit.Initialize(
            problem,
            parameters.SwarmSize,
            random,
            ref evaluations);

    ReadOnlySpan<double> lower =
        problem.SearchSpace.LowerBounds;

    ReadOnlySpan<double> upper =
        problem.SearchSpace.UpperBounds;

    for (int iteration = 0;
         iteration < parameters.MaximumIterations;
         iteration++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        double adaptiveRadius =
        AdaptiveNicheRadius(swarm);

        for (int i = 0; i < swarm.Count; i++)
        {
            double[] leaderPosition =
                swarm[AdaptiveLeader(swarm, i, adaptiveRadius, problem.Sense)].PersonalBest;

            for (int d = 0;
                 d < swarm[i].Position.Length;
                 d++)
            {
                swarm[i].Velocity[d] =
                    parameters.Inertia *
                    swarm[i].Velocity[d] +
                    parameters.Cognitive *
                    random.NextDouble() *
                    (swarm[i].PersonalBest[d] -
                     swarm[i].Position[d]) +
                    parameters.Social *
                    random.NextDouble() *
                    (leaderPosition[d] -
                     swarm[i].Position[d]);

                swarm[i].Position[d] =
                    Math.Clamp(
                        swarm[i].Position[d] +
                        swarm[i].Velocity[d],
                        lower[d],
                        upper[d]);
            }

            swarm[i].Objective =
                problem.Evaluate(
                    swarm[i].Position);

            evaluations++;

            if (MultimodalToolkit.Better(
                    swarm[i].Objective,
                    swarm[i].PersonalBestObjective,
                    problem.Sense))
            {
                swarm[i].PersonalBestObjective =
                    swarm[i].Objective;

                Array.Copy(
                    swarm[i].Position,
                    swarm[i].PersonalBest,
                    swarm[i].Position.Length);
            }
        }
    }

    List<MultimodalCandidate> archive =
        swarm
            .Select(candidate =>
                new MultimodalCandidate(
                    (double[])candidate.PersonalBest.Clone(),
                    candidate.PersonalBestObjective))
            .ToList();

    double resultRadius =
        Math.Max(AdaptiveNicheRadius(swarm), parameters.NicheRadius);

    return new MultimodalOptimizationResult(
        MultimodalToolkit.ExtractDistinctOptima(
            archive,
            problem.Sense,
            resultRadius,
            parameters.MaximumOptima),
        evaluations,
        parameters.MaximumIterations,
        seed);
}

    private static double AdaptiveNicheRadius(
        IReadOnlyList<MultimodalCandidate> swarm)
    {
        return Math.Max(
            MultimodalToolkit.MedianNearestNeighborDistance(
                swarm),
            1e-12);
    }

    private static int AdaptiveLeader(
        IReadOnlyList<MultimodalCandidate> swarm,
        int index,
        double radius,
        OptimizationSense sense)
    {
        int best = index;
        double bestKey =
            MultimodalToolkit.Key(
                swarm[index].PersonalBestObjective,
                sense);

        for (int j = 0; j < swarm.Count; j++)
        {
            if (MultimodalToolkit.Distance(
                    swarm[index].Position,
                    swarm[j].Position) > radius)
                continue;

            double key =
                MultimodalToolkit.Key(
                    swarm[j].PersonalBestObjective,
                    sense);

            if (key < bestKey)
            {
                best = j;
                bestKey = key;
            }
        }

        return best;
    }

}
