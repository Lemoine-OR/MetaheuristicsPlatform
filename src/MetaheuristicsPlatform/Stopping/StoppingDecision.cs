namespace MetaheuristicsPlatform.Stopping;

/// <summary>
/// Result of evaluating a stopping criterion.
/// </summary>
public readonly record struct StoppingDecision(
    bool ShouldStop,
    string Criterion,
    string? Message = null)
{
    /// <summary>Creates a continue decision.</summary>
    public static StoppingDecision Continue(string criterion) => new(false, criterion);

    /// <summary>Creates a stop decision.</summary>
    public static StoppingDecision Stop(string criterion, string? message = null) =>
        new(true, criterion, message);
}