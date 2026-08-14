using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Trajectory.Acceptance;

public readonly record struct TrajectoryAcceptanceContext(
    OptimizationSense Sense,
    long Iteration,
    double CurrentObjective,
    double CandidateObjective,
    double BestObjective)
{
    public TrajectoryTransitionQuality Quality =>
        TrajectoryObjectiveComparison.Classify(
            Sense,
            CandidateObjective,
            CurrentObjective);
}