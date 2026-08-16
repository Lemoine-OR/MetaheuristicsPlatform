namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Domain-owned distance used by Skewed Variable Neighborhood Search.
/// </summary>
public interface ISolutionDistance<TSolution>
{
    /// <summary>Returns a finite non-negative distance between two feasible solutions.</summary>
    double Distance(
        in TSolution first,
        in TSolution second);
}

/// <summary>Observable state shared by the advanced VNS variants.</summary>
public readonly record struct AdvancedVariableNeighborhoodSearchState(
    string Variant,
    int CyclesCompleted,
    int NeighborhoodIndex,
    int NeighborhoodCount,
    int AcceptedCandidates,
    long AcceptedLocalMoves,
    int SkewedAcceptances);
