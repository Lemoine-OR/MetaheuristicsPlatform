namespace MetaheuristicsPlatform.Algorithms.DE;

/// <summary>
/// Classical Differential Evolution mutation strategies.
/// </summary>
public enum DeMutationStrategy
{
    Rand1 = 0,
    Best1 = 1,
    CurrentToBest1 = 2,
    Rand2 = 3
}