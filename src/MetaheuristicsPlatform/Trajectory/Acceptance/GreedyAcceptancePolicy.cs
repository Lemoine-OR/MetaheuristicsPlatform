using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Trajectory.Acceptance;

/// <summary>
/// Accepts improving moves and, optionally, equal-objective moves.
/// </summary>
public sealed class GreedyAcceptancePolicy :
    ITrajectoryAcceptancePolicy
{
    public GreedyAcceptancePolicy(
        bool acceptEqual = false)
    {
        AcceptEqual = acceptEqual;
    }

    public bool AcceptEqual { get; }

    public bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        return context.Quality switch
        {
            TrajectoryTransitionQuality.Improving =>
                true,

            TrajectoryTransitionQuality.Equal =>
                AcceptEqual,

            TrajectoryTransitionQuality.Worsening =>
                false,

            _ =>
                throw new ArgumentOutOfRangeException()
        };
    }
}