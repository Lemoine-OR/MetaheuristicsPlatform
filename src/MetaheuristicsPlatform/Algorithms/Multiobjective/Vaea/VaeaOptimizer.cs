using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Vaea;

public sealed class VaeaOptimizer :
    IMultiobjectiveOptimizer<VaeaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Vaea,
            Name = "Vector Angle-Based Evolutionary Algorithm",
            Acronym = "VaEA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { VaeaReferences.XiangZhouLiChen2017 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        VaeaParameters parameters,
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

            int needed =
                size -
                selected.Count;

            selected.AddRange(
                AngleSelection(
                    front,
                    candidates,
                    senses,
                    needed));

            break;
        }

        return selected
            .Select(MultiobjectiveToolkit.Clone)
            .ToList();
    }

    private static IEnumerable<MoCandidate> AngleSelection(
        IReadOnlyList<MoCandidate> front,
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses,
        int needed)
    {
        List<MoCandidate> pool =
            front.ToList();

        List<MoCandidate> chosen = new();

        if (pool.Count == 0 ||
            needed <= 0)
            return chosen;

        int objectives = senses.Count;

        for (int objective = 0;
             objective < objectives &&
             chosen.Count < needed;
             objective++)
        {
            int index = objective;

            MoCandidate extreme =
                pool
                    .OrderBy(
                        candidate =>
                            MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                                candidate,
                                population,
                                senses)[index])
                    .First();

            if (!chosen.Contains(extreme))
                chosen.Add(extreme);
        }

        foreach (MoCandidate item in chosen.ToArray())
            pool.Remove(item);

        while (chosen.Count < needed &&
               pool.Count > 0)
        {
            int bestIndex = 0;
            double bestAngle =
                double.NegativeInfinity;

            for (int i = 0; i < pool.Count; i++)
            {
                double[] candidateDirection =
                    MultiobjectiveAdvancedToolkit.UnitDirection(
                        pool[i],
                        population,
                        senses);

                double minimumAngle =
                    double.PositiveInfinity;

                foreach (MoCandidate current in chosen)
                {
                    double[] currentDirection =
                        MultiobjectiveAdvancedToolkit.UnitDirection(
                            current,
                            population,
                            senses);

                    minimumAngle =
                        Math.Min(
                            minimumAngle,
                            MultiobjectiveAdvancedToolkit.VectorAngle(
                                candidateDirection,
                                currentDirection));
                }

                if (chosen.Count == 0)
                    minimumAngle = Math.PI;

                double convergence =
                    MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                        pool[i],
                        population,
                        senses)
                    .Sum();

                double score =
                    minimumAngle -
                    1e-3 *
                    convergence;

                if (score > bestAngle)
                {
                    bestAngle = score;
                    bestIndex = i;
                }
            }

            chosen.Add(pool[bestIndex]);
            pool.RemoveAt(bestIndex);
        }

        return chosen;
    }
}
