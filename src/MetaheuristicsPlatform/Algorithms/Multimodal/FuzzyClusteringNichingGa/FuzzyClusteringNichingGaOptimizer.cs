using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.FuzzyClusteringNichingGa;

public sealed class FuzzyClusteringNichingGaOptimizer :
    IMultimodalOptimizer<FuzzyClusteringNichingGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.FuzzyClusteringNichingGa,
            Name = "Fuzzy-Clustering Niching Genetic Algorithm",
            Acronym = "FCN-GA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    FuzzyClusteringNichingGaOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
    IContinuousMultimodalOptimizationProblem problem,
    FuzzyClusteringNichingGaParameters parameters,
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
            SelectByFuzzyClusters(union, parameters.PopulationSize, parameters.ClusterCount, parameters.Fuzziness, problem.Sense);
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

    private static List<MultimodalCandidate> SelectByFuzzyClusters(
        IReadOnlyList<MultimodalCandidate> population,
        int populationSize,
        int clusterCount,
        double fuzziness,
        OptimizationSense sense)
    {
        int k =
            Math.Min(
                clusterCount,
                population.Count);

        int[] assignment =
            FuzzyClusterAssignment(
                population,
                k,
                fuzziness);

        List<MultimodalCandidate> selected =
            new(populationSize);

        int perCluster =
            Math.Max(
                1,
                populationSize / k);

        for (int cluster = 0; cluster < k; cluster++)
            selected.AddRange(
                population
                    .Where((candidate, index) =>
                        assignment[index] == cluster)
                    .OrderBy(candidate =>
                        MultimodalToolkit.Key(
                            candidate.Objective,
                            sense))
                    .Take(perCluster));

        foreach (MultimodalCandidate candidate in
            population.OrderBy(item =>
                MultimodalToolkit.Key(
                    item.Objective,
                    sense)))
        {
            if (selected.Count >= populationSize)
                break;

            if (!selected.Contains(candidate))
                selected.Add(candidate);
        }

        return selected
            .Take(populationSize)
            .ToList();
    }

    private static int[] FuzzyClusterAssignment(
        IReadOnlyList<MultimodalCandidate> population,
        int clusterCount,
        double fuzziness)
    {
        double[][] centers =
            population
                .Take(clusterCount)
                .Select(candidate =>
                    (double[])candidate.Position.Clone())
                .ToArray();

        int[] assignment =
            new int[population.Count];

        for (int iteration = 0; iteration < 5; iteration++)
        {
            double[][] weighted =
                centers
                    .Select(center =>
                        new double[center.Length])
                    .ToArray();

            double[] weights =
                new double[clusterCount];

            for (int i = 0; i < population.Count; i++)
            {
                double[] memberships =
                    new double[clusterCount];

                double sum = 0.0;

                for (int c = 0; c < clusterCount; c++)
                {
                    double distance =
                        Math.Max(
                            MultimodalToolkit.Distance(
                                population[i].Position,
                                centers[c]),
                            1e-12);

                    memberships[c] =
                        Math.Pow(
                            1.0 / distance,
                            2.0 / (fuzziness - 1.0));

                    sum += memberships[c];
                }

                int best = 0;
                double bestMembership = -1.0;

                for (int c = 0; c < clusterCount; c++)
                {
                    double membership =
                        memberships[c] / sum;

                    if (membership > bestMembership)
                    {
                        bestMembership = membership;
                        best = c;
                    }

                    double weight =
                        Math.Pow(
                            membership,
                            fuzziness);

                    weights[c] += weight;

                    for (int d = 0;
                         d < weighted[c].Length;
                         d++)
                        weighted[c][d] +=
                            weight *
                            population[i].Position[d];
                }

                assignment[i] = best;
            }

            for (int c = 0; c < clusterCount; c++)
                if (weights[c] > 0.0)
                    for (int d = 0;
                         d < centers[c].Length;
                         d++)
                        centers[c][d] =
                            weighted[c][d] /
                            weights[c];
        }

        return assignment;
    }

}
