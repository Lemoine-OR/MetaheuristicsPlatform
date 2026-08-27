using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaIII;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.ThetaDea;

public sealed class ThetaDeaOptimizer :
    IMultiobjectiveOptimizer<ThetaDeaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ThetaDea,
            Name = "Theta-Dominance Evolutionary Algorithm",
            Acronym = "theta-DEA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { ThetaDeaReferences.YuanXuWangYao2016 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        ThetaDeaParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        IRandomSource random =
            MultiobjectiveToolkit.CreateRandom(
                options,
                out ulong seed);

        int evaluations = 0;

        List<MoCandidate> population =
            MultiobjectiveToolkit.Initialize(
                problem,
                parameters.PopulationSize,
                random,
                ref evaluations);

        double[][] references =
            ReferenceDirectionUtilities.DasDennis(
                problem.ObjectiveCount,
                parameters.ReferenceDivisions);

        double mutationProbability =
            parameters.MutationProbability < 0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AssignThetaFitness(
                population,
                references,
                parameters.Theta,
                problem.ObjectiveSenses);

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    population[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            population,
                            random)];

                MoCandidate second =
                    population[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            population,
                            random)];

                double[] child =
                    MultiobjectiveToolkit.SbxChild(
                        first.Position,
                        second.Position,
                        problem.SearchSpace,
                        random,
                        parameters.CrossoverProbability,
                        parameters.DistributionIndex);

                MultiobjectiveToolkit.PolynomialMutate(
                    child,
                    problem.SearchSpace,
                    random,
                    mutationProbability,
                    parameters.DistributionIndex);

                problem.SearchSpace.Clamp(child);

                offspring.Add(
                    MultiobjectiveToolkit.Evaluate(
                        problem,
                        child,
                        ref evaluations));
            }

            List<MoCandidate> union =
                new(population.Count + offspring.Count);

            union.AddRange(population);
            union.AddRange(offspring);

            AssignThetaFitness(
                union,
                references,
                parameters.Theta,
                problem.ObjectiveSenses);

            population =
                union
                    .OrderBy(candidate => candidate.Fitness)
                    .Take(parameters.PopulationSize)
                    .Select(MultiobjectiveToolkit.Clone)
                    .ToList();
        }

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                population,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static void AssignThetaFitness(
        IReadOnlyList<MoCandidate> candidates,
        double[][] references,
        double theta,
        IReadOnlyList<OptimizationSense> senses)
    {
        double[][] normalized =
            candidates
                .Select(
                    candidate =>
                        MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                            candidate,
                            candidates,
                            senses))
                .ToArray();

        Dictionary<int, List<int>> clusters = new();
        double[] pbi = new double[candidates.Count];

        for (int i = 0; i < candidates.Count; i++)
        {
            var association =
                ReferenceDirectionUtilities.Associate(
                    normalized[i],
                    references);

            pbi[i] =
                MultiobjectiveAdvancedToolkit.Pbi(
                    normalized[i],
                    references[association.Reference],
                    theta);

            if (!clusters.TryGetValue(
                    association.Reference,
                    out List<int>? indices))
            {
                indices = new();
                clusters[association.Reference] = indices;
            }

            indices.Add(i);
        }

        foreach (List<int> cluster in clusters.Values)
        {
            int rank = 0;

            foreach (int index in
                cluster.OrderBy(
                    value => pbi[value]))
            {
                candidates[index].Fitness =
                    rank +
                    pbi[index] /
                    (1.0 + pbi[index]);

                rank++;
            }
        }
    }
}
