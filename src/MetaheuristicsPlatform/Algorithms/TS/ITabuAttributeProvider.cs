namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Maps domain moves to solution attributes used by short-term tabu memory.
/// </summary>
/// <remarks>
/// Candidate and registered attributes are deliberately distinct. For an assignment move,
/// the candidate attribute can represent the value being entered while the registered
/// attribute can represent the value just left, thereby forbidding an immediate reversal.
/// </remarks>
public interface ITabuAttributeProvider<TSolution, TMove, TAttribute>
    where TAttribute : notnull
{
    TAttribute GetCandidateAttribute(
        in TSolution solution,
        in TMove move);

    TAttribute GetAttributeToForbid(
        in TSolution solution,
        in TMove move);
}
