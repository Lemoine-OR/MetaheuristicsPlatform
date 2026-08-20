using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>Creates the starting solution for stochastic acceptance-based trajectories.</summary>
public interface IAcceptanceTrajectoryInitialSolutionGenerator<TSolution>
{
    TSolution Create(IOptimizationProblem<TSolution> problem, IRandomSource random);
}

/// <summary>Delegate-backed acceptance-trajectory initial-solution generator.</summary>
public sealed class DelegateAcceptanceTrajectoryInitialSolutionGenerator<TSolution> :
    IAcceptanceTrajectoryInitialSolutionGenerator<TSolution>
{
    private readonly Func<IOptimizationProblem<TSolution>, IRandomSource, TSolution> _factory;

    public DelegateAcceptanceTrajectoryInitialSolutionGenerator(
        Func<IOptimizationProblem<TSolution>, IRandomSource, TSolution> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public TSolution Create(IOptimizationProblem<TSolution> problem, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(random);
        return _factory(problem, random);
    }
}