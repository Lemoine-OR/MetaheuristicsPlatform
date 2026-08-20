using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Trajectory;

/// <summary>
/// Registers an already evaluated trajectory candidate while distinguishing a probe
/// from an accepted/visited state.
/// </summary>
/// <remarks>
/// Every candidate consumes the common evaluation budget. Only an accepted visited
/// candidate may promote the best-so-far snapshot. This distinction is required by
/// classical Great Deluge because its absolute water level can reject a candidate
/// even when that candidate improves the current state.
/// </remarks>
public static class TrajectoryStepEvaluationAccounting
{
    public static long RegisterVisitedStep<TSolution>(
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        in TSolution currentSolution,
        in TrajectoryStepResult step)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        long evaluationIndex =
            context.RegisterExternalProbeEvaluation(step.CandidateObjective, step);

        if (step.Accepted && context.WouldImprove(step.CandidateObjective))
        {
            TSolution snapshot = solutionCloner.Clone(currentSolution);

            context.PromoteOwnedExternalProbeSnapshot(
                snapshot,
                step.CandidateObjective,
                evaluationIndex,
                step);
        }

        return evaluationIndex;
    }
}