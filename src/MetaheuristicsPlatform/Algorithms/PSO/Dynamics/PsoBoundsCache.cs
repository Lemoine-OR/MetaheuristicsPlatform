using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>Cached array form of immutable continuous bounds for parallel kernels.</summary>
internal sealed class PsoBoundsCache
{
    internal PsoBoundsCache(
        IBoundedContinuousSearchSpace searchSpace)
    {
        Lower = searchSpace.LowerBounds.ToArray();
        Upper = searchSpace.UpperBounds.ToArray();
    }

    internal double[] Lower { get; }
    internal double[] Upper { get; }
}