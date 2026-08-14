namespace MetaheuristicsPlatform.Trajectory.Moves;

/// <summary>
/// Applies a move to a solution.
/// </summary>
/// <remarks>
/// The solution is passed by ref so the operator can mutate mutable representations
/// or replace immutable/value-type representations.
/// </remarks>
public interface IMoveOperator<TSolution, TMove>
{
    void Apply(
        ref TSolution solution,
        in TMove move);
}