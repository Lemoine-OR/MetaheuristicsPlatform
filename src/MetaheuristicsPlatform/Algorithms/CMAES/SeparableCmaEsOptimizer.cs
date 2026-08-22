using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// Separable CMA-ES of Ros and Hansen (2008), constraining covariance
/// adaptation to coordinate-wise variances and linear internal complexity.
/// </summary>
public sealed class SeparableCmaEsOptimizer :
    IMetaheuristic<double[], CmaEsParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SeparableCmaEs,
            Name = "Separable CMA-ES",
            Acronym = "sep-CMA-ES",
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
                CmaEsReferences.RosHansen2008,
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
            AdvancedCmaEsMode.SeparableCovariance,
            problem,
            parameters,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
}
