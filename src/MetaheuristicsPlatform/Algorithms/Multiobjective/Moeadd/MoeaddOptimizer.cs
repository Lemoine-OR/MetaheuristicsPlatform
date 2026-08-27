using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaIII;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Moeadd;

public sealed class MoeaddOptimizer :
    IMultiobjectiveOptimizer<MoeaddParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Moeadd,
            Name = "MOEA/DD",
            Acronym = "MOEA/DD",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { MoeaddReferences.LiDebZhangKwong2015 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        MoeaddParameters parameters,
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

            MultiobjectiveToolkit.SortFronts(
                population,
                problem.ObjectiveSenses);

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    MultiobjectiveToolkit.Tournament(
                        population,
                        random);

                MoCandidate second =
                    MultiobjectiveToolkit.Tournament(
                        population,
                        random);

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
                Select(
                    union,
                    parameters.PopulationSize,
                    references,
                    parameters.PbiTheta,
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

    private static List<MoCandidate> Select(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        double[][] references,
        double theta,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<List<MoCandidate>> fronts =
            MultiobjectiveToolkit.SortFronts(
                candidates,
                senses);

        List<MoCandidate> selected =
            new(size);

        foreach (List<MoCandidate> front in fronts)
        {
            if (selected.Count + front.Count <= size)
            {
                selected.AddRange(front);
                continue;
            }

            int needed =
                size -
                selected.Count;

            selected.AddRange(
                DecompositionPick(
                    front,
                    candidates,
                    references,
                    theta,
                    senses,
                    needed));

            break;
        }

        return selected
            .Select(MultiobjectiveToolkit.Clone)
            .ToList();
    }

    private static IEnumerable<MoCandidate> DecompositionPick(
        IReadOnlyList<MoCandidate> front,
        IReadOnlyList<MoCandidate> population,
        double[][] references,
        double theta,
        IReadOnlyList<OptimizationSense> senses,
        int needed)
    {
        Dictionary<int, List<(MoCandidate Candidate, double Value)>> regions =
            new();

        foreach (MoCandidate candidate in front)
        {
            double[] normalized =
                MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                    candidate,
                    population,
                    senses);

            var association =
                ReferenceDirectionUtilities.Associate(
                    normalized,
                    references);

            double value =
                MultiobjectiveAdvancedToolkit.Pbi(
                    normalized,
                    references[association.Reference],
                    theta);

            if (!regions.TryGetValue(
                    association.Reference,
                    out List<(MoCandidate Candidate, double Value)>? list))
            {
                list = new();
                regions[association.Reference] = list;
            }

            list.Add((candidate, value));
        }

        List<MoCandidate> chosen = new();

        while (chosen.Count < needed &&
               regions.Count > 0)
        {
            foreach (int region in regions.Keys.ToArray())
            {
                if (chosen.Count >= needed)
                    break;

                List<(MoCandidate Candidate, double Value)> list =
                    regions[region];

                if (list.Count == 0)
                {
                    regions.Remove(region);
                    continue;
                }

                int best =
                    Enumerable.Range(0, list.Count)
                        .OrderBy(index => list[index].Value)
                        .First();

                chosen.Add(
                    list[best].Candidate);

                list.RemoveAt(best);

                if (list.Count == 0)
                    regions.Remove(region);
            }
        }

        return chosen;
    }
}
