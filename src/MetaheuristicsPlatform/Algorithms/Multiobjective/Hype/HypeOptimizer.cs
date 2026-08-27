using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Hype;

public sealed class HypeOptimizer :
    IMultiobjectiveOptimizer<HypeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Hype,
            Name = "Hypervolume Estimation Algorithm",
            Acronym = "HypE",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { HypeReferences.BaderZitzler2011 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        HypeParameters parameters,
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

            AssignEstimatedHypervolumeFitness(
                population,
                parameters.HypervolumeSamples,
                problem.ObjectiveSenses,
                random);

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    population[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            population,
                            random,
                            smallerIsBetter: false)];

                MoCandidate second =
                    population[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            population,
                            random,
                            smallerIsBetter: false)];

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
                    parameters.HypervolumeSamples,
                    problem.ObjectiveSenses,
                    random);
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
        IReadOnlyList<MoCandidate> union,
        int size,
        int samples,
        IReadOnlyList<OptimizationSense> senses,
        IRandomSource random)
    {
        List<MoCandidate> working =
            union
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        while (working.Count > size)
        {
            List<List<MoCandidate>> fronts =
                MultiobjectiveToolkit.SortFronts(
                    working,
                    senses);

            List<MoCandidate> last =
                fronts[^1];

            if (last.Count == 1)
            {
                working.Remove(last[0]);
                continue;
            }

            AssignEstimatedHypervolumeFitness(
                last,
                samples,
                senses,
                random);

            MoCandidate remove =
                last.OrderBy(
                        candidate =>
                            candidate.Fitness)
                    .First();

            working.Remove(remove);
        }

        return working;
    }

    private static void AssignEstimatedHypervolumeFitness(
        IReadOnlyList<MoCandidate> candidates,
        int samples,
        IReadOnlyList<OptimizationSense> senses,
        IRandomSource random)
    {
        if (candidates.Count == 0)
            return;

        int objectives = senses.Count;
        double[] lower =
            Enumerable.Repeat(
                double.PositiveInfinity,
                objectives)
            .ToArray();

        double[] upper =
            Enumerable.Repeat(
                double.NegativeInfinity,
                objectives)
            .ToArray();

        double[][] points =
            new double[candidates.Count][];

        for (int i = 0; i < candidates.Count; i++)
        {
            points[i] =
                new double[objectives];

            for (int objective = 0;
                 objective < objectives;
                 objective++)
            {
                double value =
                    MultiobjectiveToolkit.Normalize(
                        candidates[i].Objectives[objective],
                        senses[objective]);

                points[i][objective] = value;
                lower[objective] =
                    Math.Min(
                        lower[objective],
                        value);

                upper[objective] =
                    Math.Max(
                        upper[objective],
                        value);
            }
        }

        for (int objective = 0;
             objective < objectives;
             objective++)
        {
            double span =
                upper[objective] -
                lower[objective];

            upper[objective] +=
                Math.Max(
                    1e-9,
                    0.1 *
                    Math.Max(
                        span,
                        1e-9));
        }

        double[] contribution =
            new double[candidates.Count];

        double[] sample =
            new double[objectives];

        for (int draw = 0; draw < samples; draw++)
        {
            for (int objective = 0;
                 objective < objectives;
                 objective++)
                sample[objective] =
                    lower[objective] +
                    (
                        upper[objective] -
                        lower[objective]) *
                    random.NextDouble();

            List<int> dominators = new();

            for (int i = 0; i < points.Length; i++)
            {
                bool dominates = true;

                for (int objective = 0;
                     objective < objectives;
                     objective++)
                    if (points[i][objective] >
                        sample[objective])
                    {
                        dominates = false;
                        break;
                    }

                if (dominates)
                    dominators.Add(i);
            }

            if (dominators.Count == 0)
                continue;

            double credit =
                1.0 /
                dominators.Count;

            foreach (int index in dominators)
                contribution[index] += credit;
        }

        for (int i = 0; i < candidates.Count; i++)
            candidates[i].Fitness =
                contribution[i] /
                samples;
    }
}
