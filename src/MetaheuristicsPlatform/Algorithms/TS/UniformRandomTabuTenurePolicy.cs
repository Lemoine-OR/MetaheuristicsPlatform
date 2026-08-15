using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Draws the tabu tenure uniformly from an inclusive integer interval.
/// </summary>
/// <remarks>
/// Randomly varying tenure is a practical dynamic-memory policy. It is not presented as
/// the reactive tenure adaptation of Battiti and Tecchiolli (1994), which requires cycle
/// detection and is intentionally outside this v0.21 foundation.
/// </remarks>
public sealed class UniformRandomTabuTenurePolicy : ITabuTenurePolicy
{
    public UniformRandomTabuTenurePolicy(
        int minimumTenure,
        int maximumTenure)
    {
        if (minimumTenure <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumTenure));
        }

        if (maximumTenure < minimumTenure ||
            maximumTenure == int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumTenure));
        }

        MinimumTenure = minimumTenure;
        MaximumTenure = maximumTenure;
    }

    public int MinimumTenure { get; }
    public int MaximumTenure { get; }

    public int GetTenure(
        in TabuTenureContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        return random.NextInt32(
            MinimumTenure,
            MaximumTenure + 1);
    }
}
