namespace MetaheuristicsPlatform.Neighborhoods;

/// <summary>
/// Enumerated neighborhood that returns a value-type cursor instead of IEnumerable.
/// </summary>
public interface IEnumeratedNeighborhood<
    TSolution,
    TMove,
    TEnumerator>
    where TEnumerator :
        struct,
        INeighborhoodEnumerator<TMove>
{
    TEnumerator GetEnumerator(
        in TSolution solution);
}