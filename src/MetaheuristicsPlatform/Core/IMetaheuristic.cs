using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Common metadata contract for every metaheuristic.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public interface IMetaheuristic<TSolution>
{
    /// <summary>Gets metadata and scientific classification for this algorithm implementation.</summary>
    MetaheuristicDescriptor Descriptor { get; }
}

/// <summary>
/// Strongly typed execution contract for a metaheuristic.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
/// <typeparam name="TParameters">Algorithm-specific parameter type.</typeparam>
public interface IMetaheuristic<TSolution, TParameters> : IMetaheuristic<TSolution>
    where TParameters : IMetaheuristicParameters
{
    /// <summary>Creates the recommended default algorithm-specific parameters.</summary>
    TParameters CreateDefaultParameters();

    /// <summary>Runs the metaheuristic using the common platform lifecycle.</summary>
    OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        TParameters parameters,
        ISolutionCloner<TSolution> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<TSolution>? callback = null,
        CancellationToken cancellationToken = default);
}