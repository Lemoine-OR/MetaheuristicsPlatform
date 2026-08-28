using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.LocallyInformedPso;

public sealed class LocallyInformedPsoOptimizer :
    IMultimodalOptimizer<LocallyInformedPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.LocallyInformedPso,
            Name = "Distance-Based Locally Informed Particle Swarm",
            Acronym = "LIPS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    LocallyInformedPsoOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    LocallyInformedPsoParameters parameters,
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
                LocallyInformedBest(swarm, i, parameters.NeighborhoodSize, problem.Sense, random);

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

    private static double[] LocallyInformedBest(
        IReadOnlyList<MultimodalCandidate> swarm,
        int index,
        int neighborhoodSize,
        OptimizationSense sense,
        IRandomSource random)
    {
        int[] nearest =
            MultimodalToolkit.NearestIndices(
                swarm,
                index,
                neighborhoodSize);

        double[] informed =
            new double[
                swarm[index].Position.Length];

        double weightSum = 0.0;

        foreach (int neighbor in nearest)
        {
            double quality =
                1.0 /
                (1.0 +
                 Math.Abs(
                     MultimodalToolkit.Key(
                         swarm[neighbor].PersonalBestObjective,
                         sense)));

            double weight =
                quality *
                (0.5 + random.NextDouble());

            weightSum += weight;

            for (int d = 0; d < informed.Length; d++)
                informed[d] +=
                    weight *
                    swarm[neighbor].PersonalBest[d];
        }

        if (weightSum <= 0.0)
            return
                (double[])swarm[index].PersonalBest.Clone();

        for (int d = 0; d < informed.Length; d++)
            informed[d] /= weightSum;

        return informed;
    }

}
