using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.SpeciesConservingGa;

public sealed class SpeciesConservingGaOptimizer :
    IMultimodalOptimizer<SpeciesConservingGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SpeciesConservingGa,
            Name = "Species Conserving Genetic Algorithm",
            Acronym = "SCGA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    SpeciesConservingGaOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    SpeciesConservingGaParameters parameters,
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
            ConserveSpeciesSeeds(union, parameters.PopulationSize, parameters.NicheRadius, problem.Sense);
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

    private static List<MultimodalCandidate> ConserveSpeciesSeeds(
        IEnumerable<MultimodalCandidate> candidates,
        int populationSize,
        double speciesRadius,
        OptimizationSense sense)
    {
        List<MultimodalCandidate> pool =
            candidates.ToList();

        List<MultimodalCandidate> seeds =
            FindSpeciesSeeds(
                pool,
                speciesRadius,
                sense);

        List<MultimodalCandidate> selected =
            new(populationSize);

        selected.AddRange(
            seeds.Take(populationSize));

        foreach (MultimodalCandidate candidate in
            pool.OrderBy(item =>
                MultimodalToolkit.Key(
                    item.Objective,
                    sense)))
        {
            if (selected.Count >= populationSize)
                break;

            if (!selected.Contains(candidate))
                selected.Add(candidate);
        }

        return selected;
    }

    private static List<MultimodalCandidate> FindSpeciesSeeds(
        IReadOnlyList<MultimodalCandidate> population,
        double speciesRadius,
        OptimizationSense sense)
    {
        List<MultimodalCandidate> seeds = new();

        foreach (MultimodalCandidate candidate in
            population.OrderBy(item =>
                MultimodalToolkit.Key(
                    item.Objective,
                    sense)))
        {
            if (seeds.All(seed =>
                MultimodalToolkit.Distance(
                    seed.Position,
                    candidate.Position) >
                speciesRadius))
                seeds.Add(candidate);
        }

        return seeds;
    }

}
