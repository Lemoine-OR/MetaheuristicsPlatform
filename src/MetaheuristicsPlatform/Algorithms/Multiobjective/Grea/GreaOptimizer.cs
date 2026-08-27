using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Grea;

public sealed class GreaOptimizer :
    IMultiobjectiveOptimizer<GreaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Grea,
            Name = "Grid-Based Evolutionary Algorithm",
            Acronym = "GrEA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { GreaReferences.YangLiLiuZheng2013 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        GreaParameters parameters,
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

        double mutationProbability =
            parameters.MutationProbability < 0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AssignGridFitness(
                population,
                parameters.GridDivisions,
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

            population =
                EnvironmentalSelection(
                    union,
                    parameters.PopulationSize,
                    parameters.GridDivisions,
                    problem.ObjectiveSenses);
        }

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                population,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static void AssignGridFitness(
        IReadOnlyList<MoCandidate> population,
        int divisions,
        IReadOnlyList<OptimizationSense> senses)
    {
        Dictionary<string, int> density = new();
        string[] cells = new string[population.Count];
        int[] gridRanks = new int[population.Count];

        for (int i = 0; i < population.Count; i++)
        {
            double[] normalized =
                MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                    population[i],
                    population,
                    senses);

            int[] coordinates = new int[normalized.Length];
            int rank = 0;

            for (int objective = 0;
                 objective < normalized.Length;
                 objective++)
            {
                coordinates[objective] =
                    Math.Clamp(
                        (int)Math.Floor(
                            normalized[objective] *
                            divisions),
                        0,
                        divisions - 1);

                rank += coordinates[objective];
            }

            string cell =
                string.Join(
                    ":",
                    coordinates);

            cells[i] = cell;
            gridRanks[i] = rank;

            density[cell] =
                density.TryGetValue(
                    cell,
                    out int count)
                    ? count + 1
                    : 1;
        }

        for (int i = 0; i < population.Count; i++)
        {
            double convergence =
                MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                    population[i],
                    population,
                    senses)
                .Sum();

            population[i].Fitness =
                gridRanks[i] +
                density[cells[i]] /
                    (double)Math.Max(
                        population.Count,
                        1) +
                1e-6 * convergence;
        }
    }

    private static List<MoCandidate> EnvironmentalSelection(
        IReadOnlyList<MoCandidate> union,
        int size,
        int divisions,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> working =
            union
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        MultiobjectiveToolkit.SortFronts(
            working,
            senses);

        AssignGridFitness(
            working,
            divisions,
            senses);

        return working
            .OrderBy(candidate => candidate.Rank)
            .ThenBy(candidate => candidate.Fitness)
            .Take(size)
            .ToList();
    }
}
