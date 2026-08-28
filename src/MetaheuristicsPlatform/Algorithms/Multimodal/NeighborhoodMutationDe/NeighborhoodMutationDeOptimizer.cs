using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.NeighborhoodMutationDe;

public sealed class NeighborhoodMutationDeOptimizer :
    IMultimodalOptimizer<NeighborhoodMutationDeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.NeighborhoodMutationDe,
            Name = "Neighborhood-Mutation Differential Evolution",
            Acronym = "NMDE",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    NeighborhoodMutationDeOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
        IContinuousMultimodalOptimizationProblem problem,
        NeighborhoodMutationDeParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        IRandomSource random =
            MultimodalToolkit.CreateRandom(options, out ulong seed);

        int evaluations = 0;
        List<MultimodalCandidate> population =
            MultimodalToolkit.Initialize(
                problem,
                parameters.PopulationSize,
                random,
                ref evaluations);

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<MultimodalCandidate> next =
                new(population);

            for (int i = 0; i < population.Count; i++)
            {
                int[] neighborhood =
                    MultimodalToolkit.NearestIndices(
                        population,
                        i,
                        Math.Max(
                            3,
                            parameters.NeighborhoodSize));

                double[] trialPosition =
                    NeighborhoodMutation(
                        population,
                        i,
                        neighborhood,
                        problem,
                        random,
                        parameters);

                MultimodalCandidate trial =
                    MultimodalToolkit.Evaluate(
                        problem,
                        trialPosition,
                        ref evaluations);

                if (MultimodalToolkit.Better(
                        trial.Objective,
                        population[i].Objective,
                        problem.Sense))
                    next[i] = trial;
            }

            population = next;
        }

        return new MultimodalOptimizationResult(
            MultimodalToolkit.ExtractDistinctOptima(
                population,
                problem.Sense,
                parameters.NicheRadius,
                parameters.MaximumOptima),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static double[] NeighborhoodMutation(
        IReadOnlyList<MultimodalCandidate> population,
        int targetIndex,
        IReadOnlyList<int> neighborhood,
        IContinuousMultimodalOptimizationProblem problem,
        IRandomSource random,
        NeighborhoodMutationDeParameters parameters)
    {
        return MultimodalToolkit.DifferentialTrial(
            population,
            targetIndex,
            neighborhood,
            problem.SearchSpace,
            random,
            parameters.DifferentialWeight,
            parameters.CrossoverProbability);
    }
}
