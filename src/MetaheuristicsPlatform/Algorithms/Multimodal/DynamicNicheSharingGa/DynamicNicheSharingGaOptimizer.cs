using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.DynamicNicheSharingGa;

public sealed class DynamicNicheSharingGaOptimizer :
    IMultimodalOptimizer<DynamicNicheSharingGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.DynamicNicheSharingGa,
            Name = "Dynamic Niche Sharing Genetic Algorithm",
            Acronym = "DNS-GA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    DynamicNicheSharingGaOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    DynamicNicheSharingGaParameters parameters,
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

        List<MultimodalCandidate> offspring =
            new(parameters.PopulationSize);

        while (offspring.Count < parameters.PopulationSize)
        {
            MultimodalCandidate first =
                population[
                    random.NextInt32(
                        population.Count)];

            MultimodalCandidate second =
                population[
                    random.NextInt32(
                        population.Count)];

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

            offspring.Add(
                MultimodalToolkit.Evaluate(
                    problem,
                    child,
                    ref evaluations));
        }

        List<MultimodalCandidate> union =
            new(population.Count + offspring.Count);

        union.AddRange(population);
        union.AddRange(offspring);

        population =
            SelectBySharedFitness(union, parameters.PopulationSize, parameters.SharingAlpha, problem.Sense);
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

    private static List<MultimodalCandidate> SelectBySharedFitness(
        IReadOnlyList<MultimodalCandidate> population,
        int populationSize,
        double alpha,
        OptimizationSense sense)
    {
        double radius =
            DynamicSharingRadius(population);

        double minimumKey =
            population.Min(candidate =>
                MultimodalToolkit.Key(
                    candidate.Objective,
                    sense));

        for (int i = 0; i < population.Count; i++)
        {
            double sharing = 0.0;

            for (int j = 0; j < population.Count; j++)
            {
                double distance =
                    MultimodalToolkit.Distance(
                        population[i].Position,
                        population[j].Position);

                if (distance >= radius)
                    continue;

                sharing +=
                    1.0 -
                    Math.Pow(
                        distance / radius,
                        alpha);
            }

            double shiftedFitness =
                MultimodalToolkit.Key(
                    population[i].Objective,
                    sense) -
                minimumKey +
                1.0;

            population[i].Score =
                shiftedFitness *
                Math.Max(sharing, 1.0);
        }

        return population
            .OrderBy(candidate => candidate.Score)
            .Take(populationSize)
            .ToList();
    }

    private static double DynamicSharingRadius(
        IReadOnlyList<MultimodalCandidate> population)
    {
        return Math.Max(
            MultimodalToolkit.MedianNearestNeighborDistance(
                population),
            1e-12);
    }

}
