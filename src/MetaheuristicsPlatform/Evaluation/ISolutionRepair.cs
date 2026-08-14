namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Repairs a decoded solution and may replace the solution value itself.
/// </summary>
/// <remarks>
/// The solution is passed by ref so the contract is correct for both mutable
/// reference types and value-type solution representations.
/// </remarks>
public interface ISolutionRepair<TSolution>
{
    /// <returns>True when the repair changed or replaced the solution.</returns>
    bool Repair(
        ref TSolution solution,
        CancellationToken cancellationToken = default);
}