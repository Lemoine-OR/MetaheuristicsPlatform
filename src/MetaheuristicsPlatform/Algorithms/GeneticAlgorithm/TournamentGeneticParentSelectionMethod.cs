using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Objective-sense symmetric tournament selection with sampling with replacement.
/// </summary>
public sealed class TournamentGeneticParentSelectionMethod<TSolution> :
    IGeneticParentSelectionMethod<TSolution>
{
    public TournamentGeneticParentSelectionMethod(
        int tournamentSize = 2)
    {
        if (tournamentSize < 2)
            throw new ArgumentOutOfRangeException(nameof(tournamentSize));

        TournamentSize = tournamentSize;
    }

    public int TournamentSize { get; }

    public int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);

        if (population.Count == 0)
        {
            throw new ArgumentException(
                "Tournament selection requires a non-empty population.",
                nameof(population));
        }

        int bestIndex =
            random.NextInt32(population.Count);

        for (int draw = 1;
             draw < TournamentSize;
             draw++)
        {
            int challengerIndex =
                random.NextInt32(population.Count);

            if (sense.IsBetter(
                    population[challengerIndex].Objective,
                    population[bestIndex].Objective))
            {
                bestIndex =
                    challengerIndex;
            }
        }

        return bestIndex;
    }
}
