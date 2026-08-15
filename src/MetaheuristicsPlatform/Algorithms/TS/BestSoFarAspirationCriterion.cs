using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Overrides tabu status when a candidate strictly improves the global best objective.
/// </summary>
public sealed class BestSoFarAspirationCriterion : ITabuAspirationCriterion
{
    public bool RequiresCandidateObjective => true;

    public bool IsAspirational(
        in TabuAspirationContext context,
        OptimizationSense sense) =>
        sense.IsBetter(
            context.CandidateObjective,
            context.BestObjective);
}
