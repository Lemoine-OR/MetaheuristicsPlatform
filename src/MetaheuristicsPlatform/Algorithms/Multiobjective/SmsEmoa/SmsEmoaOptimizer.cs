using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.SmsEmoa;
public sealed class SmsEmoaOptimizer : IMultiobjectiveOptimizer<SmsEmoaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.SmsEmoa,
        Name = "SMS-EMOA",
        Acronym = "SMS-EMOA",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { SmsEmoaReferences.BeumeNaujoksEmmerich2007 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        SmsEmoaParameters parameters,
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
        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MultiobjectiveToolkit.SortFronts(population, problem.ObjectiveSenses);
            MoCandidate first = MultiobjectiveToolkit.Tournament(population, random);
            MoCandidate second = MultiobjectiveToolkit.Tournament(population, random);
            double[] child = MultiobjectiveToolkit.SbxChild(first.Position, second.Position, problem.SearchSpace, random, parameters.CrossoverProbability, parameters.DistributionIndex);
            MultiobjectiveToolkit.PolynomialMutate(child, problem.SearchSpace, random, pm, parameters.DistributionIndex);
            problem.SearchSpace.Clamp(child);
            population.Add(MultiobjectiveToolkit.Evaluate(problem, child, ref evaluations));
            List<List<MoCandidate>> fronts = MultiobjectiveToolkit.SortFronts(population, problem.ObjectiveSenses);
            List<MoCandidate> worstFront = fronts[^1];
            int removeIndex;
            if (worstFront.Count == 1)
                removeIndex = population.IndexOf(worstFront[0]);
            else
            {
                int local = Enumerable.Range(0, worstFront.Count)
                    .OrderBy(index => HypervolumeUtilities.Contribution(worstFront, index, problem.ObjectiveSenses))
                    .First();
                removeIndex = population.IndexOf(worstFront[local]);
            }
            population.RemoveAt(removeIndex);
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(population, problem.ObjectiveSenses),
            evaluations, parameters.MaximumIterations, seed);
    }
}
