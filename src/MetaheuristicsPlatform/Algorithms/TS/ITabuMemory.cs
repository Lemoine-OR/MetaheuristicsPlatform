namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Short-term tabu memory indexed by domain attributes.
/// </summary>
public interface ITabuMemory<TAttribute>
    where TAttribute : notnull
{
    int Count { get; }

    void Advance(long iteration);

    bool IsTabu(
        in TAttribute attribute,
        long iteration);

    void Register(
        in TAttribute attribute,
        long tabuUntilIteration);
}
