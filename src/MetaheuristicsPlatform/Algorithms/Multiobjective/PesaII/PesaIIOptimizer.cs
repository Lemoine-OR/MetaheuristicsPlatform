using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.PesaII;
public sealed class PesaIIOptimizer : IMultiobjectiveOptimizer<PesaIIParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.PesaII,
        Name = "PESA-II",
        Acronym = "PESA-II",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { PesaIIReferences.CorneJerramKnowlesOates2001 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        PesaIIParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        List<MoCandidate> population = MultiobjectiveToolkit.Initialize(problem, parameters.PopulationSize, random, ref evaluations);
        List<MoCandidate> archive = new();
        foreach (MoCandidate candidate in population)
            MultiobjectiveToolkit.InsertGridArchive(archive, candidate, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
        double pm = parameters.MutationProbability < 0 ? 1.0 / problem.SearchSpace.Dimension : parameters.MutationProbability;
        for (int generation = 0; generation < parameters.MaximumGenerations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<MoCandidate> next = new(parameters.PopulationSize);
            while (next.Count < parameters.PopulationSize)
            {
                MoCandidate first = MultiobjectiveToolkit.SelectAdaptiveGridLeader(archive, parameters.GridDivisions, problem.ObjectiveSenses, random);
                MoCandidate second = MultiobjectiveToolkit.SelectAdaptiveGridLeader(archive, parameters.GridDivisions, problem.ObjectiveSenses, random);
                double[] child = MultiobjectiveToolkit.SbxChild(first.Position, second.Position, problem.SearchSpace, random, parameters.CrossoverProbability, parameters.DistributionIndex);
                MultiobjectiveToolkit.PolynomialMutate(child, problem.SearchSpace, random, pm, parameters.DistributionIndex);
                problem.SearchSpace.Clamp(child);
                MoCandidate evaluated = MultiobjectiveToolkit.Evaluate(problem, child, ref evaluations);
                next.Add(evaluated);
                MultiobjectiveToolkit.InsertGridArchive(archive, evaluated, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
            }
            population = next;
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(archive, problem.ObjectiveSenses),
            evaluations, parameters.MaximumGenerations, seed);
    }
}
