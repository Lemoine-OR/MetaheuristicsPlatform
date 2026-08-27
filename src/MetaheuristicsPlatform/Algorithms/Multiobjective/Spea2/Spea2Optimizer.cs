using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Spea2;

public sealed class Spea2Optimizer :
    IMultiobjectiveOptimizer<Spea2Parameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Spea2,
            Name = "Strength Pareto Evolutionary Algorithm 2",
            Acronym = "SPEA2",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { Spea2References.ZitzlerLaumannsThiele2001 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        Spea2Parameters parameters,
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

        List<MoCandidate> archive = new();

        double mutationProbability =
            parameters.MutationProbability < 0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<MoCandidate> union =
                new(population.Count + archive.Count);

            union.AddRange(population);
            union.AddRange(archive);

            AssignFitness(
                union,
                problem.ObjectiveSenses);

            archive =
                EnvironmentalSelection(
                    union,
                    parameters.ArchiveSize,
                    problem.ObjectiveSenses);

            AssignFitness(
                archive,
                problem.ObjectiveSenses);

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    archive[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            archive,
                            random)];

                MoCandidate second =
                    archive[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            archive,
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

            population = offspring;
        }

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                archive,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static void AssignFitness(
        IReadOnlyList<MoCandidate> candidates,
        IReadOnlyList<OptimizationSense> senses)
    {
        int n = candidates.Count;
        int[] strength = new int[n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (i != j &&
                    ParetoDominance.Compare(
                        candidates[i].Objectives,
                        candidates[j].Objectives,
                        senses) < 0)
                    strength[i]++;

        int k =
            Math.Max(
                1,
                (int)Math.Sqrt(n));

        for (int i = 0; i < n; i++)
        {
            double raw = 0.0;

            for (int j = 0; j < n; j++)
                if (i != j &&
                    ParetoDominance.Compare(
                        candidates[j].Objectives,
                        candidates[i].Objectives,
                        senses) < 0)
                    raw += strength[j];

            List<double> distances = new();

            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    continue;

                distances.Add(
                    MultiobjectiveAdvancedToolkit.ObjectiveDistance(
                        candidates[i],
                        candidates[j],
                        candidates,
                        senses));
            }

            distances.Sort();

            double sigma =
                distances.Count == 0
                    ? double.PositiveInfinity
                    : distances[
                        Math.Min(
                            k - 1,
                            distances.Count - 1)];

            double density =
                1.0 /
                (sigma + 2.0);

            candidates[i].Fitness =
                raw + density;
        }
    }

    private static List<MoCandidate> EnvironmentalSelection(
        IReadOnlyList<MoCandidate> union,
        int archiveSize,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> selected =
            union
                .Where(candidate => candidate.Fitness < 1.0)
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        if (selected.Count > archiveSize)
            return MultiobjectiveAdvancedToolkit.TruncateByNearestNeighbor(
                selected,
                archiveSize,
                senses);

        if (selected.Count < archiveSize)
        {
            IEnumerable<MoCandidate> remaining =
                union
                    .Where(candidate => candidate.Fitness >= 1.0)
                    .OrderBy(candidate => candidate.Fitness);

            foreach (MoCandidate candidate in remaining)
            {
                if (selected.Count >= archiveSize)
                    break;

                selected.Add(
                    MultiobjectiveToolkit.Clone(
                        candidate));
            }
        }

        return selected;
    }
}
