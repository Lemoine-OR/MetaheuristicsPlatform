using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Parameters shared by first- and best-improvement local search.</summary>
public sealed class LocalSearchParameters : IMetaheuristicParameters
{
    public int MaximumAcceptedMoves { get; init; } = int.MaxValue;

    public void Validate()
    {
        if (MaximumAcceptedMoves <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumAcceptedMoves));
        }
    }
}
