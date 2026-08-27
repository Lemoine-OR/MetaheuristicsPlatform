using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Paes;
public sealed class PaesOptimizer : IMultiobjectiveOptimizer<PaesParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.Paes,
        Name = "Pareto Archived Evolution Strategy",
        Acronym = "PAES",
        SolutionModel = MetaheuristicSolutionModel.SingleSolution,
        Families = MetaheuristicFamily.Evolutionary | MetaheuristicFamily.LocalSearch,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { PaesReferences.KnowlesCorne2000 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        PaesParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        int d = problem.SearchSpace.Dimension;
        double[] x = new double[d];
        problem.SearchSpace.Sample(random, x);
        MoCandidate current = MultiobjectiveToolkit.Evaluate(problem, x, ref evaluations);
        List<MoCandidate> archive = new();
        MultiobjectiveToolkit.InsertGridArchive(archive, current, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
        double pm = parameters.MutationProbability < 0.0 ? 1.0 / d : parameters.MutationProbability;
        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double[] y = (double[])current.Position.Clone();
            MultiobjectiveToolkit.PolynomialMutate(y, problem.SearchSpace, random, pm, parameters.MutationDistributionIndex);
            problem.SearchSpace.Clamp(y);
            MoCandidate candidate = MultiobjectiveToolkit.Evaluate(problem, y, ref evaluations);
            int cmp = ParetoDominance.Compare(candidate.Objectives, current.Objectives, problem.ObjectiveSenses);
            if (cmp < 0)
                current = candidate;
            else if (cmp == 0)
            {
                MultiobjectiveToolkit.InsertGridArchive(archive, candidate, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
                int currentDensity = MultiobjectiveToolkit.GridDensity(archive, current, parameters.GridDivisions, problem.ObjectiveSenses);
                int candidateDensity = MultiobjectiveToolkit.GridDensity(archive, candidate, parameters.GridDivisions, problem.ObjectiveSenses);
                if (candidateDensity <= currentDensity) current = candidate;
            }
            MultiobjectiveToolkit.InsertGridArchive(archive, current, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(archive, problem.ObjectiveSenses),
            evaluations, parameters.MaximumIterations, seed);
    }
}
