using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Internal generation-level extension point shared by the canonical GA and hybrid
/// population algorithms. The public GA runs with no extension.
/// </summary>
internal interface IGeneticAlgorithmExecutionExtension<TSolution>
{
    /// <summary>Maps canonical GA execution counters to the algorithm-specific callback state.</summary>
    object CreateAlgorithmState(in GeneticAlgorithmState state);

    /// <summary>
    /// Applies an optional post-generation transformation after all offspring have been
    /// evaluated and before generational replacement becomes visible to the next iteration.
    /// </summary>
    StoppingDecision ProcessCompletedGeneration(
        List<GeneticPopulationMember<TSolution>> nextPopulation,
        in GeneticAlgorithmState state,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken);

    /// <summary>Receives the final global-improvement outcome of a completed generation.</summary>
    void CompleteGeneration(bool improvedGlobalBest);
}
