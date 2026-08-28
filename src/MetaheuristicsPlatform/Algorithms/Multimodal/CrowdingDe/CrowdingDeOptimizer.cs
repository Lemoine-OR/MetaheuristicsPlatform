using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.CrowdingDe;

public sealed class CrowdingDeOptimizer :
    IMultimodalOptimizer<CrowdingDeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CrowdingDe,
            Name = "Crowding Differential Evolution",
            Acronym = "CrowdingDE",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    CrowdingDeOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
        IContinuousMultimodalOptimizationProblem problem,
        CrowdingDeParameters parameters,
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

        int[] fullPool =
            Enumerable.Range(0, population.Count)
                .ToArray();

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int i = 0; i < population.Count; i++)
            {
                double[] trialPosition =
                    MultimodalToolkit.DifferentialTrial(
                        population,
                        i,
                        fullPool,
                        problem.SearchSpace,
                        random,
                        parameters.DifferentialWeight,
                        parameters.CrossoverProbability);

                MultimodalCandidate trial =
                    MultimodalToolkit.Evaluate(
                        problem,
                        trialPosition,
                        ref evaluations);

                int competitor =
                    ClosestCompetitorIndex(
                        population,
                        trial.Position);

                if (MultimodalToolkit.Better(
                        trial.Objective,
                        population[competitor].Objective,
                        problem.Sense))
                    population[competitor] = trial;
            }
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

    private static int ClosestCompetitorIndex(
        IReadOnlyList<MultimodalCandidate> population,
        ReadOnlySpan<double> position)
    {
        return MultimodalToolkit.ClosestIndex(
            population,
            position);
    }
}
