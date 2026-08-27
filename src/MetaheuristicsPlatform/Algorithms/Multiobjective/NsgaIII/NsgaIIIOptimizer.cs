using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaIII;
public sealed class NsgaIIIOptimizer : IMultiobjectiveOptimizer<NsgaIIIParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.NsgaIII,
        Name = "NSGA-III",
        Acronym = "NSGA-III",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { NsgaIIIReferences.DebJain2014 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        NsgaIIIParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        List<MoCandidate> population = MultiobjectiveToolkit.Initialize(problem, parameters.PopulationSize, random, ref evaluations);
        double[][] references = ReferenceDirectionUtilities.DasDennis(problem.ObjectiveCount, parameters.ReferenceDivisions);
        double pm = parameters.MutationProbability < 0 ? 1.0 / problem.SearchSpace.Dimension : parameters.MutationProbability;
        for (int generation = 0; generation < parameters.MaximumGenerations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MultiobjectiveToolkit.SortFronts(population, problem.ObjectiveSenses);
            List<MoCandidate> children = new(parameters.PopulationSize);
            while (children.Count < parameters.PopulationSize)
            {
                MoCandidate a = MultiobjectiveToolkit.Tournament(population, random);
                MoCandidate b = MultiobjectiveToolkit.Tournament(population, random);
                double[] child = MultiobjectiveToolkit.SbxChild(a.Position, b.Position, problem.SearchSpace, random, parameters.CrossoverProbability, parameters.DistributionIndex);
                MultiobjectiveToolkit.PolynomialMutate(child, problem.SearchSpace, random, pm, parameters.DistributionIndex);
                problem.SearchSpace.Clamp(child);
                children.Add(MultiobjectiveToolkit.Evaluate(problem, child, ref evaluations));
            }
            List<MoCandidate> combined = new(population.Count + children.Count);
            combined.AddRange(population);
            combined.AddRange(children);
            population = Select(combined, parameters.PopulationSize, problem.ObjectiveSenses, references, random);
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(population, problem.ObjectiveSenses),
            evaluations, parameters.MaximumGenerations, seed);
    }
    private static List<MoCandidate> Select(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        IReadOnlyList<OptimizationSense> senses,
        double[][] references,
        IRandomSource random)
    {
        List<List<MoCandidate>> fronts = MultiobjectiveToolkit.SortFronts(candidates, senses);
        List<MoCandidate> selected = new(size);
        List<MoCandidate>? lastFront = null;
        foreach (List<MoCandidate> front in fronts)
        {
            if (selected.Count + front.Count <= size)
                selected.AddRange(front);
            else
            {
                lastFront = front;
                break;
            }
        }
        if (lastFront is null || selected.Count == size)
            return selected.Take(size).ToList();
        List<MoCandidate> pool = new(selected.Count + lastFront.Count);
        pool.AddRange(selected);
        pool.AddRange(lastFront);
        double[][] normalized = ReferenceDirectionUtilities.NormalizeObjectives(pool, senses);
        Dictionary<MoCandidate, (int Reference, double Distance)> association = new();
        for (int i = 0; i < pool.Count; i++)
        {
            var item = ReferenceDirectionUtilities.Associate(normalized[i], references);
            association[pool[i]] = item;
        }
        int[] nicheCount = new int[references.Length];
        foreach (MoCandidate candidate in selected)
            nicheCount[association[candidate].Reference]++;
        List<MoCandidate> remaining = new(lastFront);
        while (selected.Count < size && remaining.Count > 0)
        {
            int minimum = remaining.Min(candidate => nicheCount[association[candidate].Reference]);
            int[] eligibleReferences = remaining
                .Select(candidate => association[candidate].Reference)
                .Distinct()
                .Where(reference => nicheCount[reference] == minimum)
                .ToArray();
            int reference = eligibleReferences[random.NextInt32(eligibleReferences.Length)];
            List<MoCandidate> niche = remaining
                .Where(candidate => association[candidate].Reference == reference)
                .ToList();
            MoCandidate chosen = nicheCount[reference] == 0
                ? niche.OrderBy(candidate => association[candidate].Distance).First()
                : niche[random.NextInt32(niche.Count)];
            selected.Add(chosen);
            remaining.Remove(chosen);
            nicheCount[reference]++;
        }
        return selected;
    }
}
