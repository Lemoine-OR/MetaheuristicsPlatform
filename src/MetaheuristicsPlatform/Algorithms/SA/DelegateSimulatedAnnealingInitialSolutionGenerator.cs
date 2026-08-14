using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.SA;

public delegate TSolution SimulatedAnnealingInitialSolutionFactory<TSolution>(
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public sealed class DelegateSimulatedAnnealingInitialSolutionGenerator<TSolution> :
    ISimulatedAnnealingInitialSolutionGenerator<TSolution>
{
    private readonly SimulatedAnnealingInitialSolutionFactory<TSolution>
        _factory;

    public DelegateSimulatedAnnealingInitialSolutionGenerator(
        SimulatedAnnealingInitialSolutionFactory<TSolution> factory)
    {
        _factory =
            factory ??
            throw new ArgumentNullException(
                nameof(factory));
    }

    public TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(random);

        return _factory(
            problem,
            random);
    }
}