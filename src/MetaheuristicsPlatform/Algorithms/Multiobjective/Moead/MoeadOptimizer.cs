using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Moead;
public sealed class MoeadOptimizer : IMultiobjectiveOptimizer<MoeadParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.Moead,
        Name = "MOEA/D",
        Acronym = "MOEA/D",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary | MetaheuristicFamily.DecompositionBased,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { MoeadReferences.ZhangLi2007 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        MoeadParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        int n = parameters.PopulationSize;
        int m = problem.ObjectiveCount;
        int d = problem.SearchSpace.Dimension;
        List<MoCandidate> population = MultiobjectiveToolkit.Initialize(problem, n, random, ref evaluations);
        double[][] weights = CreateWeights(n, m, random);
        int[][] neighborhoods = CreateNeighborhoods(weights, parameters.NeighborhoodSize);
        double[] ideal = Enumerable.Repeat(double.PositiveInfinity, m).ToArray();
        UpdateIdeal(population, ideal, problem.ObjectiveSenses);
        for (int generation = 0; generation < parameters.MaximumGenerations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int i = 0; i < n; i++)
            {
                int[] neighbors = neighborhoods[i];
                int r1 = neighbors[random.NextInt32(neighbors.Length)];
                int r2 = neighbors[random.NextInt32(neighbors.Length)];
                while (r2 == r1) r2 = neighbors[random.NextInt32(neighbors.Length)];
                int r3 = neighbors[random.NextInt32(neighbors.Length)];
                while (r3 == r1 || r3 == r2) r3 = neighbors[random.NextInt32(neighbors.Length)];
                double[] child = (double[])population[r1].Position.Clone();
                int forced = random.NextInt32(d);
                for (int coordinate = 0; coordinate < d; coordinate++)
                    if (coordinate == forced || random.NextDouble() < parameters.CrossoverProbability)
                        child[coordinate] =
                            population[r1].Position[coordinate] +
                            parameters.DifferentialWeight *
                            (population[r2].Position[coordinate] - population[r3].Position[coordinate]);
                problem.SearchSpace.Clamp(child);
                MoCandidate candidate = MultiobjectiveToolkit.Evaluate(problem, child, ref evaluations);
                UpdateIdeal(new[] { candidate }, ideal, problem.ObjectiveSenses);
                foreach (int neighbor in neighbors)
                    if (MultiobjectiveToolkit.Tchebycheff(candidate.Objectives, weights[neighbor], ideal, problem.ObjectiveSenses) <=
                        MultiobjectiveToolkit.Tchebycheff(population[neighbor].Objectives, weights[neighbor], ideal, problem.ObjectiveSenses))
                        population[neighbor] = MultiobjectiveToolkit.Clone(candidate);
            }
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(population, problem.ObjectiveSenses),
            evaluations, parameters.MaximumGenerations, seed);
    }
    private static double[][] CreateWeights(int n, int m, IRandomSource random)
    {
        double[][] weights = new double[n][];
        for (int i = 0; i < n; i++)
        {
            weights[i] = new double[m];
            if (m == 2)
            {
                weights[i][0] = i / (double)(n - 1);
                weights[i][1] = 1.0 - weights[i][0];
            }
            else
            {
                double sum = 0.0;
                for (int j = 0; j < m; j++)
                {
                    weights[i][j] = -Math.Log(Math.Max(random.NextDouble(), double.Epsilon));
                    sum += weights[i][j];
                }
                for (int j = 0; j < m; j++) weights[i][j] /= sum;
            }
        }
        return weights;
    }
    private static int[][] CreateNeighborhoods(double[][] weights, int size)
    {
        int n = weights.Length;
        int[][] result = new int[n][];
        for (int i = 0; i < n; i++)
        {
            int index = i;
            result[i] = Enumerable.Range(0, n)
                .OrderBy(j => SquaredDistance(weights[index], weights[j]))
                .Take(size)
                .ToArray();
        }
        return result;
    }
    private static double SquaredDistance(double[] first, double[] second)
    {
        double sum = 0.0;
        for (int i = 0; i < first.Length; i++)
        {
            double delta = first[i] - second[i];
            sum += delta * delta;
        }
        return sum;
    }
    private static void UpdateIdeal(
        IEnumerable<MoCandidate> candidates,
        double[] ideal,
        IReadOnlyList<OptimizationSense> senses)
    {
        foreach (MoCandidate candidate in candidates)
            for (int objective = 0; objective < ideal.Length; objective++)
                ideal[objective] = Math.Min(
                    ideal[objective],
                    MultiobjectiveToolkit.Normalize(candidate.Objectives[objective], senses[objective]));
    }
}
