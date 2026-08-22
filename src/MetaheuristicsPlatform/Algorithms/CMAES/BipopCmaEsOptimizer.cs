using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// BIPOP-CMA-ES of Hansen (2009): interlaces the IPOP large-population
/// regime with randomized small-population restarts and balances the two
/// regimes by their cumulative objective-evaluation budgets.
/// </summary>
public sealed class BipopCmaEsOptimizer :
    IMetaheuristic<double[], RestartCmaEsParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.BipopCmaEs,
            Name = "BIPOP-CMA-ES",
            Acronym = "BIPOP-CMA-ES",
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
                CmaEsReferences.Hansen2009Bipop,
                CmaEsReferences.AugerHansen2005,
                CmaEsReferences.HansenOstermeier2001
            ]
        };

    public RestartCmaEsParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        RestartCmaEsParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default) =>
        RestartCmaEsKernel.Optimize(
            Descriptor,
            RestartCmaEsStrategy.Bipop,
            problem,
            parameters,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
}
