using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Neighborhoods;

/// <summary>
/// Samples an applicable move from the neighborhood of the current solution.
/// </summary>
/// <remarks>
/// TMove is intentionally unconstrained so high-performance move descriptions can be
/// readonly structs with no heap allocation.
/// </remarks>
public interface IStochasticNeighborhood<TSolution, TMove>
{
    bool TrySampleMove(
        in TSolution solution,
        IRandomSource random,
        out TMove move);
}