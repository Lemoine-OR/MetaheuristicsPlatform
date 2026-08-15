using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Decides whether a tabu candidate is allowed to override its tabu status.
/// </summary>
public interface ITabuAspirationCriterion
{
    /// <summary>
    /// Gets whether a tabu candidate objective must be evaluated before the criterion
    /// can reject it. False enables a zero-evaluation fast rejection path.
    /// </summary>
    bool RequiresCandidateObjective { get; }

    bool IsAspirational(
        in TabuAspirationContext context,
        OptimizationSense sense);
}
