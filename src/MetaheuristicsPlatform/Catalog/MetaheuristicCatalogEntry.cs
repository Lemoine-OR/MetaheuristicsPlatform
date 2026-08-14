namespace MetaheuristicsPlatform.Catalog;

public sealed record MetaheuristicCatalogEntry(
    string Id,
    string Name,
    string ClassName,
    string Category,
    string Family,
    string TimeComplexity,
    string SpaceComplexity,
    string Applicability,
    bool RequiresComposition,
    string SourcePath,
    string Publication,
    string Doi,
    string Implementation);
