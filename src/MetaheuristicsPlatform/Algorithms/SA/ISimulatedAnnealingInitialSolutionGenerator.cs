using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Creates the starting point of a simulated-annealing trajectory.
/// </summary>
/// <remarks>
/// This responsibility is intentionally separate from the optimization problem.
/// A generator may be random, constructive, heuristic, decoder-based, or deterministic.
/// </remarks>
public interface ISimulatedAnnealingInitialSolutionGenerator<TSolution>
{
    TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}