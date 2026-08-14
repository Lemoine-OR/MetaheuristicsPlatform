namespace MetaheuristicsPlatform.Trajectory.Moves;

/// <summary>
/// Move operator that can restore the exact pre-move solution without cloning it.
/// </summary>
public interface IReversibleMoveOperator<
    TSolution,
    TMove,
    TUndo> :
    IMoveOperator<TSolution, TMove>
{
    TUndo CaptureUndo(
        in TSolution solution,
        in TMove move);

    void Undo(
        ref TSolution solution,
        in TMove move,
        in TUndo undo);
}