using MetaheuristicsPlatform.Algorithms.Acceptance;
using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.TA;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

/// <summary>
/// Adapts a trajectory acceptance policy to the LNS/ALNS acceptance contract.
/// </summary>
public sealed class TrajectoryAcceptanceLargeNeighborhoodAdapter :
    ILargeNeighborhoodAcceptancePolicy
{
    private readonly ITrajectoryAcceptancePolicy _policy;

    public TrajectoryAcceptanceLargeNeighborhoodAdapter(
        ITrajectoryAcceptancePolicy policy)
    {
        _policy =
            policy ??
            throw new ArgumentNullException(nameof(policy));
    }

    public bool ShouldAccept(
        in LargeNeighborhoodAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var trajectoryContext =
            new TrajectoryAcceptanceContext(
                context.Sense,
                context.Iteration,
                context.CurrentObjective,
                context.CandidateObjective,
                context.BestObjective);

        return
            _policy.ShouldAccept(
                in trajectoryContext,
                random);
    }
}

/// <summary>Convenience constructors for acceptance criteria studied in advanced ALNS.</summary>
public static class AdvancedAdaptiveLargeNeighborhoodAcceptance
{
    public static ILargeNeighborhoodAcceptancePolicy Threshold(
        double threshold) =>
        new TrajectoryAcceptanceLargeNeighborhoodAdapter(
            new ThresholdAcceptancePolicy(
                threshold));

    public static ILargeNeighborhoodAcceptancePolicy RecordToRecordTravel(
        double deviation) =>
        new TrajectoryAcceptanceLargeNeighborhoodAdapter(
            new RecordToRecordTravelAcceptancePolicy(
                deviation));

    public static ILargeNeighborhoodAcceptancePolicy FromTrajectoryPolicy(
        ITrajectoryAcceptancePolicy policy) =>
        new TrajectoryAcceptanceLargeNeighborhoodAdapter(
            policy);
}
