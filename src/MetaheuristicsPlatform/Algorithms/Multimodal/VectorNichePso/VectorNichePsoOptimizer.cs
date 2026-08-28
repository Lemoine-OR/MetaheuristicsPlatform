using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.VectorNichePso;

public sealed class VectorNichePsoOptimizer :
    IMultimodalOptimizer<VectorNichePsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.VectorNichePso,
            Name = "Vector-Niche Particle Swarm Optimization",
            Acronym = "VNPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    VectorNichePsoOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    VectorNichePsoParameters parameters,
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



        for (int i = 0; i < swarm.Count; i++)
        {
            double[] leaderPosition =
                swarm[VectorNicheBest(swarm, i, parameters.NicheRadius, problem.Sense)].PersonalBest;

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
        parameters.NicheRadius;

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

    private static int VectorNicheBest(
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
            if (j == index)
                continue;

            double distance =
                MultimodalToolkit.Distance(
                    swarm[index].Position,
                    swarm[j].Position);

            if (distance > radius)
                continue;

            double dot = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int d = 0;
                 d < swarm[index].Position.Length;
                 d++)
            {
                double a =
                    swarm[index].PersonalBest[d] -
                    swarm[index].Position[d];

                double b =
                    swarm[j].PersonalBest[d] -
                    swarm[index].Position[d];

                dot += a * b;
                normA += a * a;
                normB += b * b;
            }

            double cosine =
                normA <= 1e-30 ||
                normB <= 1e-30
                    ? 1.0
                    : dot /
                      Math.Sqrt(
                          normA * normB);

            if (cosine < 0.0)
                continue;

            double key =
                MultimodalToolkit.Key(
                    swarm[j].PersonalBestObjective,
                    sense);

            if (key < bestKey)
            {
                bestKey = key;
                best = j;
            }
        }

        return best;
    }

}
