using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.TS;

public delegate TSolution TabuSearchInitialSolutionFactory<TSolution>(
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public sealed class DelegateTabuSearchInitialSolutionGenerator<TSolution> :
    ITabuSearchInitialSolutionGenerator<TSolution>
{
    private readonly TabuSearchInitialSolutionFactory<TSolution> _factory;

    public DelegateTabuSearchInitialSolutionGenerator(
        TabuSearchInitialSolutionFactory<TSolution> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(random);
        return _factory(problem, random);
    }
}
