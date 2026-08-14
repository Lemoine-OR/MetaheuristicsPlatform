namespace MetaheuristicsPlatform.Neighborhoods;

/// <summary>
/// Allocation-free cursor contract for enumerated neighborhoods.
/// </summary>
public interface INeighborhoodEnumerator<TMove>
{
    bool MoveNext(
        out TMove move);
}