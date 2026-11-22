using System.Buffers;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Ibea;
public sealed class IbeaOptimizer : IMultiobjectiveOptimizer<IbeaParameters>
{
    private const int MaximumCachedPopulation = 256;

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.Ibea,
        Name = "Indicator-Based Evolutionary Algorithm",
        Acronym = "IBEA",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { IbeaReferences.ZitzlerKunzli2004 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        IbeaParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        List<MoCandidate> population = MultiobjectiveToolkit.Initialize(problem, parameters.PopulationSize, random, ref evaluations);
        double pm = parameters.MutationProbability < 0 ? 1.0 / problem.SearchSpace.Dimension : parameters.MutationProbability;
        for (int generation = 0; generation < parameters.MaximumGenerations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AssignFitness(population, problem.ObjectiveSenses, parameters.Kappa);
            List<MoCandidate> children = new(parameters.PopulationSize);
            while (children.Count < parameters.PopulationSize)
            {
                MoCandidate a = IndicatorTournament(population, random);
                MoCandidate b = IndicatorTournament(population, random);
                double[] child = MultiobjectiveToolkit.SbxChild(a.Position, b.Position, problem.SearchSpace, random, parameters.CrossoverProbability, parameters.DistributionIndex);
                MultiobjectiveToolkit.PolynomialMutate(child, problem.SearchSpace, random, pm, parameters.DistributionIndex);
                problem.SearchSpace.Clamp(child);
                children.Add(MultiobjectiveToolkit.Evaluate(problem, child, ref evaluations));
            }
            population.AddRange(children);
            if (problem is ContinuousMultiobjectiveOptimizationProblem standardProblem &&
                population.Count <= MaximumCachedPopulation)
            {
                TrimWithCachedContributions(
                    population, parameters.PopulationSize, standardProblem.ObjectiveSenses, parameters.Kappa);
                continue;
            }

            while (population.Count > parameters.PopulationSize)
            {
                AssignFitness(population, problem.ObjectiveSenses, parameters.Kappa);
                int worst = Enumerable.Range(0, population.Count)
                    .OrderByDescending(index => population[index].Fitness)
                    .First();
                population.RemoveAt(worst);
            }
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(population, problem.ObjectiveSenses),
            evaluations, parameters.MaximumGenerations, seed);
    }
    private static void AssignFitness(
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses,
        double kappa)
    {
        for (int i = 0; i < population.Count; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < population.Count; j++)
            {
                if (i == j) continue;
                sum += ComputeContribution(population[i], population[j], senses, kappa);
            }
            population[i].Fitness = sum;
        }
    }
    private static void TrimWithCachedContributions(
        List<MoCandidate> population,
        int targetCount,
        IReadOnlyList<OptimizationSense> senses,
        double kappa)
    {
        int count = population.Count;
        double[] contributions = ArrayPool<double>.Shared.Rent(count * count);
        try
        {
            Span<int> survivors = stackalloc int[count];
            for (int i = 0; i < count; i++)
            {
                survivors[i] = i;
                int row = i * count;
                for (int j = 0; j < count; j++)
                {
                    if (i != j)
                        contributions[row + j] = ComputeContribution(population[i], population[j], senses, kappa);
                }
            }

            while (survivors.Length > targetCount)
            {
                int worst = 0;
                double worstFitness = double.NaN;
                for (int i = 0; i < survivors.Length; i++)
                {
                    int row = survivors[i] * count;
                    double sum = 0.0;

                    // Re-sum in population order to preserve floating-point rounding.
                    for (int j = 0; j < i; j++)
                        sum += contributions[row + survivors[j]];
                    for (int j = i + 1; j < survivors.Length; j++)
                        sum += contributions[row + survivors[j]];

                    population[i].Fitness = sum;
                    if (sum.CompareTo(worstFitness) > 0)
                    {
                        worst = i;
                        worstFitness = sum;
                    }
                }

                population.RemoveAt(worst);
                survivors[(worst + 1)..].CopyTo(survivors[worst..]);
                survivors = survivors[..^1];
            }
        }
        finally
        {
            ArrayPool<double>.Shared.Return(contributions);
        }
    }

    private static double ComputeContribution(
        MoCandidate candidate,
        MoCandidate other,
        IReadOnlyList<OptimizationSense> senses,
        double kappa)
    {
        double indicator = double.NegativeInfinity;
        for (int objective = 0; objective < senses.Count; objective++)
        {
            double value =
                MultiobjectiveToolkit.Normalize(other.Objectives[objective], senses[objective]) -
                MultiobjectiveToolkit.Normalize(candidate.Objectives[objective], senses[objective]);
            indicator = Math.Max(indicator, value);
        }

        return Math.Exp(-indicator / kappa);
    }

    private static MoCandidate IndicatorTournament(IReadOnlyList<MoCandidate> population, IRandomSource random)
    {
        MoCandidate a = population[random.NextInt32(population.Count)];
        MoCandidate b = population[random.NextInt32(population.Count)];
        return a.Fitness <= b.Fitness ? a : b;
    }
}
