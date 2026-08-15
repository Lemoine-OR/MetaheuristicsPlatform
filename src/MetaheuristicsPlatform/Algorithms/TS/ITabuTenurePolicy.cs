using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Chooses how many future iterations a registered attribute remains tabu.
/// </summary>
public interface ITabuTenurePolicy
{
    int GetTenure(
        in TabuTenureContext context,
        IRandomSource random);
}
