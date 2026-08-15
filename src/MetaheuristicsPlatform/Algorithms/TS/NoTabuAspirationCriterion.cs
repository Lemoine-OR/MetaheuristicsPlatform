using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Disables aspiration. Tabu candidates can therefore be rejected before objective evaluation.
/// </summary>
public sealed class NoTabuAspirationCriterion : ITabuAspirationCriterion
{
    public bool RequiresCandidateObjective => false;

    public bool IsAspirational(
        in TabuAspirationContext context,
        OptimizationSense sense) =>
        false;
}
