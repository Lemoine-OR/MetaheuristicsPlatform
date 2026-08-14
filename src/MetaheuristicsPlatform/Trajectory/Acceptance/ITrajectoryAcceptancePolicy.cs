using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Trajectory.Acceptance;

/// <summary>
/// Decides whether a candidate transition is accepted.
/// </summary>
/// <remarks>
/// Policies may be stateful. The generic contract deliberately does not prescribe
/// temperature, tabu tenure, aspiration, or another algorithm-specific control state.
/// </remarks>
public interface ITrajectoryAcceptancePolicy
{
    bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random);
}