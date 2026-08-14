namespace MetaheuristicsPlatform.Graphs;

/// <summary>
/// Structural metrics for an undirected neighborhood graph.
/// Self-loops are excluded from structural degree, density and path metrics.
/// </summary>
public readonly record struct GraphMetrics(
    int NodeCount,
    int EdgeCount,
    int SelfLoopCount,
    int ConnectedComponents,
    int MinimumDegree,
    int MaximumDegree,
    double AverageDegree,
    double DegreeVariance,
    double Density,
    int Diameter,
    double AveragePathLength,
    double AverageClusteringCoefficient);