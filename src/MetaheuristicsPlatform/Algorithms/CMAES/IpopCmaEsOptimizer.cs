using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// IPOP-CMA-ES of Auger and Hansen (2005): every restart increases
/// the CMA-ES population size geometrically, canonically by a factor of two.
/// </summary>
public sealed class IpopCmaEsOptimizer :
    IMetaheuristic<double[], RestartCmaEsParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.IpopCmaEs,
            Name = "IPOP-CMA-ES",
            Acronym = "IPOP-CMA-ES",
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
            RestartCmaEsStrategy.Ipop,
            problem,
            parameters,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
}
