using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Knea;

public sealed class KneaOptimizer :
    IMultiobjectiveOptimizer<KneaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Knea,
            Name = "Knee Point Driven Evolutionary Algorithm",
            Acronym = "KnEA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { KneaReferences.ZhangTianJin2015 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        KneaParameters parameters,
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

            AssignKneeFitness(
                population,
                parameters.KneeNeighbors,
                parameters.KneePreference,
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
                    parameters.KneeNeighbors,
                    parameters.KneePreference,
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

    private static List<MoCandidate> EnvironmentalSelection(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        int neighbors,
        double preference,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<List<MoCandidate>> fronts =
            MultiobjectiveToolkit.SortFronts(
                candidates,
                senses);

        List<MoCandidate> selected = new(size);

        foreach (List<MoCandidate> front in fronts)
        {
            if (selected.Count + front.Count <= size)
            {
                selected.AddRange(front);
                continue;
            }

            AssignKneeFitness(
                front,
                neighbors,
                preference,
                senses);

            selected.AddRange(
                front
                    .OrderBy(candidate => candidate.Fitness)
                    .ThenByDescending(candidate => candidate.Crowding)
                    .Take(size - selected.Count));

            break;
        }

        return selected
            .Select(MultiobjectiveToolkit.Clone)
            .ToList();
    }

    private static void AssignKneeFitness(
        IReadOnlyList<MoCandidate> candidates,
        int neighbors,
        double preference,
        IReadOnlyList<OptimizationSense> senses)
    {
        MultiobjectiveToolkit.SortFronts(
            candidates,
            senses);

        double[][] normalized =
            candidates
                .Select(
                    candidate =>
                        MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                            candidate,
                            candidates,
                            senses))
                .ToArray();

        for (int i = 0; i < candidates.Count; i++)
        {
            double convergence =
                normalized[i].Sum();

            int neighborCount =
                Math.Min(
                    neighbors,
                    Math.Max(
                        candidates.Count - 1,
                        0));

            int[] nearest =
                Enumerable.Range(0, candidates.Count)
                    .Where(index => index != i)
                    .OrderBy(
                        index =>
                            Euclidean(
                                normalized[i],
                                normalized[index]))
                    .Take(neighborCount)
                    .ToArray();

            bool knee =
                nearest.Length > 0;

            double localGain = 0.0;

            foreach (int neighbor in nearest)
            {
                double neighborConvergence =
                    normalized[neighbor].Sum();

                if (convergence >
                    neighborConvergence)
                    knee = false;

                localGain +=
                    Math.Max(
                        0.0,
                        neighborConvergence -
                        convergence);
            }

            localGain /=
                Math.Max(
                    nearest.Length,
                    1);

            double kneeBonus =
                knee
                    ? 1.0 + localGain
                    : 0.0;

            candidates[i].Fitness =
                candidates[i].Rank +
                (1.0 - preference) *
                convergence -
                preference *
                kneeBonus;
        }
    }

    private static double Euclidean(
        ReadOnlySpan<double> first,
        ReadOnlySpan<double> second)
    {
        double sum = 0.0;

        for (int i = 0; i < first.Length; i++)
        {
            double delta = first[i] - second[i];
            sum += delta * delta;
        }

        return Math.Sqrt(sum);
    }
}
