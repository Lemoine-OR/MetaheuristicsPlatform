using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Creates the starting point of a Tabu Search trajectory.
/// </summary>
public interface ITabuSearchInitialSolutionGenerator<TSolution>
{
    TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}
