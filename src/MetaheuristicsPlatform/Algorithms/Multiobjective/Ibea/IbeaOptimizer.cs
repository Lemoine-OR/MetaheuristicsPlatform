using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Ibea;
public sealed class IbeaOptimizer : IMultiobjectiveOptimizer<IbeaParameters>
{
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
                double indicator = double.NegativeInfinity;
                for (int objective = 0; objective < senses.Count; objective++)
                {
                    double value =
                        MultiobjectiveToolkit.Normalize(population[j].Objectives[objective], senses[objective]) -
                        MultiobjectiveToolkit.Normalize(population[i].Objectives[objective], senses[objective]);
                    indicator = Math.Max(indicator, value);
                }
                sum += Math.Exp(-indicator / kappa);
            }
            population[i].Fitness = sum;
        }
    }
    private static MoCandidate IndicatorTournament(IReadOnlyList<MoCandidate> population, IRandomSource random)
    {
        MoCandidate a = population[random.NextInt32(population.Count)];
        MoCandidate b = population[random.NextInt32(population.Count)];
        return a.Fitness <= b.Fitness ? a : b;
    }
}
