using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// Weighted Active CMA-ES using negative covariance information from
/// unsuccessful ranked offspring while preserving the common CMA lifecycle.
/// </summary>
public sealed class ActiveCmaEsOptimizer :
    IMetaheuristic<double[], CmaEsParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ActiveCmaEs,
            Name = "Active CMA-ES",
            Acronym = "aCMA-ES",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms =
                MetaheuristicMechanism.EvolutionaryOperators |
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.MemoryBased,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                CmaEsReferences.HansenRos2010,
                CmaEsReferences.JastrebskiArnold2006,
                CmaEsReferences.HansenOstermeier2001
            ]
        };

    public CmaEsParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        CmaEsParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default) =>
        AdvancedCmaEsKernel.Optimize(
            Descriptor,
            AdvancedCmaEsMode.ActiveFullCovariance,
            problem,
            parameters,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
}
