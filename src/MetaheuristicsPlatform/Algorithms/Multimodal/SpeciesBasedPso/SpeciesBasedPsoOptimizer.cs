using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.SpeciesBasedPso;

public sealed class SpeciesBasedPsoOptimizer :
    IMultimodalOptimizer<SpeciesBasedPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SpeciesBasedPso,
            Name = "Species-Based Particle Swarm Optimization",
            Acronym = "SPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    SpeciesBasedPsoOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    SpeciesBasedPsoParameters parameters,
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

        int[] leader =
        BuildSpecies(
            swarm,
            parameters.NicheRadius,
            problem.Sense);

        for (int i = 0; i < swarm.Count; i++)
        {
            double[] leaderPosition =
                swarm[leader[i]].PersonalBest;

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

    private static int[] BuildSpecies(
        IReadOnlyList<MultimodalCandidate> swarm,
        double radius,
        OptimizationSense sense)
    {
        int[] leader =
            Enumerable.Repeat(-1, swarm.Count)
                .ToArray();

        int[] order =
            Enumerable.Range(0, swarm.Count)
                .OrderBy(index =>
                    MultimodalToolkit.Key(
                        swarm[index].Objective,
                        sense))
                .ToArray();

        List<int> seeds = new();

        foreach (int index in order)
        {
            int chosenSeed = -1;

            foreach (int seed in seeds)
            {
                if (MultimodalToolkit.Distance(
                        swarm[seed].Position,
                        swarm[index].Position) <= radius)
                {
                    chosenSeed = seed;
                    break;
                }
            }

            if (chosenSeed < 0)
            {
                seeds.Add(index);
                chosenSeed = index;
            }

            leader[index] = chosenSeed;
        }

        return leader;
    }

}
