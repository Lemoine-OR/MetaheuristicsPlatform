namespace MetaheuristicsPlatform.Trajectory.Moves;

/// <summary>
/// Optional exact objective evaluator for a move without applying the move.
/// </summary>
/// <remarks>
/// Returning true promises that candidateObjective is the exact objective that a full
/// evaluation would return after applying the move.
/// </remarks>
public interface IMoveObjectiveDeltaEvaluator<TSolution, TMove>
{
    bool TryEvaluateCandidateObjective(
        in TSolution solution,
        double currentObjective,
        in TMove move,
        out double candidateObjective);
}