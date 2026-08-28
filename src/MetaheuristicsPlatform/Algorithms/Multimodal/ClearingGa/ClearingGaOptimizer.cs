using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.ClearingGa;

public sealed class ClearingGaOptimizer :
    IMultimodalOptimizer<ClearingGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ClearingGa,
            Name = "Clearing Genetic Algorithm",
            Acronym = "Clearing-GA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    ClearingGaOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    ClearingGaParameters parameters,
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
            ApplyClearing(union, parameters.PopulationSize, parameters.NicheRadius, parameters.NicheCapacity, problem.Sense);
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

    private static List<MultimodalCandidate> ApplyClearing(
        IEnumerable<MultimodalCandidate> candidates,
        int populationSize,
        double nicheRadius,
        int nicheCapacity,
        OptimizationSense sense)
    {
        List<MultimodalCandidate> ordered =
            candidates
                .OrderBy(candidate =>
                    MultimodalToolkit.Key(
                        candidate.Objective,
                        sense))
                .ToList();

        foreach (MultimodalCandidate candidate in ordered)
            candidate.Cleared = false;

        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Cleared)
                continue;

            int winners = 1;

            for (int j = i + 1; j < ordered.Count; j++)
            {
                if (ordered[j].Cleared)
                    continue;

                if (MultimodalToolkit.Distance(
                        ordered[i].Position,
                        ordered[j].Position) >
                    nicheRadius)
                    continue;

                if (winners < nicheCapacity)
                {
                    winners++;
                }
                else
                {
                    ordered[j].Cleared = true;
                }
            }
        }

        return ordered
            .OrderBy(candidate => candidate.Cleared ? 1 : 0)
            .ThenBy(candidate =>
                MultimodalToolkit.Key(
                    candidate.Objective,
                    sense))
            .Take(populationSize)
            .ToList();
    }

}
