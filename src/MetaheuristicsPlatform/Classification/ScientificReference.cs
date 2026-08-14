namespace MetaheuristicsPlatform.Classification;

/// <summary>
/// Bibliographic reference attached to an algorithm implementation or variant.
/// </summary>
public sealed record ScientificReference(
    string Authors,
    int Year,
    string Title,
    string? Venue = null,
    string? Doi = null);