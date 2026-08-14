namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Improves a decoded solution, for example with local search, and may replace
/// the solution value itself.
/// </summary>
/// <remarks>
/// The solution is passed by ref so the contract is correct for both mutable
/// reference types and value-type solution representations.
/// </remarks>
public interface ISolutionImprover<TSolution>
{
    /// <returns>True when the improvement stage changed or replaced the solution.</returns>
    bool Improve(
        ref TSolution solution,
        CancellationToken cancellationToken = default);
}