using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.MoeadDe;

public sealed class MoeadDeOptimizer :
    IMultiobjectiveOptimizer<MoeadDeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.MoeadDe,
            Name = "MOEA/D-DE",
            Acronym = "MOEA/D-DE",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { MoeadDeReferences.LiZhang2009 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        MoeadDeParameters parameters,
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
        int n = parameters.PopulationSize;
        int d = problem.SearchSpace.Dimension;

        List<MoCandidate> population =
            MultiobjectiveToolkit.Initialize(
                problem,
                n,
                random,
                ref evaluations);

        double[][] weights =
            CreateWeights(
                n,
                problem.ObjectiveCount,
                random);

        int[][] neighborhoods =
            CreateNeighborhoods(
                weights,
                parameters.NeighborhoodSize);

        double[] ideal =
            MultiobjectiveAdvancedToolkit.IdealPoint(
                population,
                problem.ObjectiveSenses);

        int[] allIndices =
            Enumerable.Range(0, n)
                .ToArray();

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int subproblem = 0;
                 subproblem < n;
                 subproblem++)
            {
                int[] pool =
                    random.NextDouble() <
                    parameters.NeighborhoodMatingProbability
                        ? neighborhoods[subproblem]
                        : allIndices;

                int first =
                    pool[
                        random.NextInt32(
                            pool.Length)];

                int second =
                    pool[
                        random.NextInt32(
                            pool.Length)];

                while (second == first)
                    second =
                        pool[
                            random.NextInt32(
                                pool.Length)];

                double[] child =
                    (double[])population[subproblem].Position.Clone();

                int forced =
                    random.NextInt32(d);

                for (int coordinate = 0;
                     coordinate < d;
                     coordinate++)
                {
                    if (coordinate == forced ||
                        random.NextDouble() <
                        parameters.CrossoverProbability)
                        child[coordinate] =
                            population[subproblem].Position[coordinate] +
                            parameters.DifferentialWeight *
                            (
                                population[first].Position[coordinate] -
                                population[second].Position[coordinate]);
                }

                problem.SearchSpace.Clamp(child);

                MoCandidate candidate =
                    MultiobjectiveToolkit.Evaluate(
                        problem,
                        child,
                        ref evaluations);

                for (int objective = 0;
                     objective < ideal.Length;
                     objective++)
                    ideal[objective] =
                        Math.Min(
                            ideal[objective],
                            MultiobjectiveToolkit.Normalize(
                                candidate.Objectives[objective],
                                problem.ObjectiveSenses[objective]));

                int replacements = 0;

                foreach (int neighbor in neighborhoods[subproblem])
                {
                    if (replacements >=
                        parameters.MaximumReplacements)
                        break;

                    double candidateValue =
                        MultiobjectiveToolkit.Tchebycheff(
                            candidate.Objectives,
                            weights[neighbor],
                            ideal,
                            problem.ObjectiveSenses);

                    double currentValue =
                        MultiobjectiveToolkit.Tchebycheff(
                            population[neighbor].Objectives,
                            weights[neighbor],
                            ideal,
                            problem.ObjectiveSenses);

                    if (candidateValue <= currentValue)
                    {
                        population[neighbor] =
                            MultiobjectiveToolkit.Clone(
                                candidate);

                        replacements++;
                    }
                }
            }
        }

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                population,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static double[][] CreateWeights(
        int count,
        int objectives,
        IRandomSource random)
    {
        double[][] weights =
            new double[count][];

        for (int i = 0; i < count; i++)
        {
            if (objectives == 2)
            {
                double first =
                    i /
                    (double)Math.Max(
                        count - 1,
                        1);

                weights[i] =
                    new[]
                    {
                        first,
                        1.0 - first
                    };
            }
            else
                weights[i] =
                    MultiobjectiveAdvancedToolkit.RandomWeights(
                        objectives,
                        random);
        }

        return weights;
    }

    private static int[][] CreateNeighborhoods(
        double[][] weights,
        int size)
    {
        int n = weights.Length;
        int[][] result = new int[n][];

        for (int i = 0; i < n; i++)
        {
            int index = i;

            result[i] =
                Enumerable.Range(0, n)
                    .OrderBy(
                        other =>
                            DistanceSquared(
                                weights[index],
                                weights[other]))
                    .Take(size)
                    .ToArray();
        }

        return result;
    }

    private static double DistanceSquared(
        ReadOnlySpan<double> first,
        ReadOnlySpan<double> second)
    {
        double sum = 0.0;

        for (int i = 0; i < first.Length; i++)
        {
            double delta = first[i] - second[i];
            sum += delta * delta;
        }

        return sum;
    }
}
