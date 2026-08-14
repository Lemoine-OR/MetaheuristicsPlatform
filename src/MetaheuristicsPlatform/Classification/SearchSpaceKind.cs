namespace MetaheuristicsPlatform.Classification;

/// <summary>
/// Search-space representations supported by an algorithm implementation.
/// </summary>
[Flags]
public enum SearchSpaceKind
{
    None = 0,
    Continuous = 1 << 0,
    Binary = 1 << 1,
    Integer = 1 << 2,
    Permutation = 1 << 3,
    Combinatorial = 1 << 4,
    Mixed = 1 << 5
}