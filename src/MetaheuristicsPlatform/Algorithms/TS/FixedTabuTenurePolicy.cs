using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>Uses a constant short-term tabu tenure.</summary>
public sealed class FixedTabuTenurePolicy : ITabuTenurePolicy
{
    public FixedTabuTenurePolicy(int tenure)
    {
        if (tenure <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenure));
        }

        Tenure = tenure;
    }

    public int Tenure { get; }

    public int GetTenure(
        in TabuTenureContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return Tenure;
    }
}
