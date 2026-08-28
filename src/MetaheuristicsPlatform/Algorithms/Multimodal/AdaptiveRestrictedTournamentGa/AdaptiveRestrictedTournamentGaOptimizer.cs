using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.AdaptiveRestrictedTournamentGa;

public sealed class AdaptiveRestrictedTournamentGaOptimizer :
    IMultimodalOptimizer<AdaptiveRestrictedTournamentGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.AdaptiveRestrictedTournamentGa,
            Name = "Adaptive Restricted Tournament Selection Genetic Algorithm",
            Acronym = "ARTS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    AdaptiveRestrictedTournamentGaOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
        IContinuousMultimodalOptimizationProblem problem,
        AdaptiveRestrictedTournamentGaParameters parameters,
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

        double mutationProbability =
            parameters.MutationProbability < 0.0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int window =
                Math.Max(
                    parameters.MinimumTournamentWindow,
                    Math.Min(
                        population.Count,
                        (int)Math.Ceiling(
                            population.Count *
                            Math.Max(
                                0.1,
                                MultimodalToolkit.MedianNearestNeighborDistance(
                                    population)))));

            for (int childIndex = 0;
                 childIndex < parameters.PopulationSize;
                 childIndex++)
            {
                MultimodalCandidate first =
                    population[random.NextInt32(population.Count)];

                MultimodalCandidate second =
                    population[random.NextInt32(population.Count)];

                double[] child =
                    MultimodalToolkit.SbxChild(
                        first.Position,
                        second.Position,
                        problem.SearchSpace,
                        random,
                        parameters.CrossoverProbability,
                        parameters.DistributionIndex);

                MultimodalToolkit.PolynomialMutate(
                    child,
                    problem.SearchSpace,
                    random,
                    mutationProbability,
                    parameters.DistributionIndex);

                MultimodalCandidate offspring =
                    MultimodalToolkit.Evaluate(
                        problem,
                        child,
                        ref evaluations);

                int competitor =
                    AdaptiveRestrictedCompetitor(
                        population,
                        offspring.Position,
                        window,
                        random);

                if (MultimodalToolkit.Better(
                        offspring.Objective,
                        population[competitor].Objective,
                        problem.Sense))
                    population[competitor] = offspring;
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

    private static int AdaptiveRestrictedCompetitor(
        IReadOnlyList<MultimodalCandidate> population,
        ReadOnlySpan<double> offspring,
        int window,
        IRandomSource random)
    {
        int best =
            random.NextInt32(population.Count);

        double bestDistance =
            MultimodalToolkit.Distance(
                population[best].Position,
                offspring);

        for (int i = 1; i < window; i++)
        {
            int index =
                random.NextInt32(population.Count);

            double distance =
                MultimodalToolkit.Distance(
                    population[index].Position,
                    offspring);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = index;
            }
        }

        return best;
    }
}
