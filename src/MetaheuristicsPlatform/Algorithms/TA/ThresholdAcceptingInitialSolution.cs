using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>Creates the starting solution of a Threshold Accepting trajectory.</summary>
public interface IThresholdAcceptingInitialSolutionGenerator<TSolution>
{
    TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>Delegate-backed Threshold Accepting initial-solution generator.</summary>
public sealed class DelegateThresholdAcceptingInitialSolutionGenerator<TSolution> :
    IThresholdAcceptingInitialSolutionGenerator<TSolution>
{
    private readonly Func<
        IOptimizationProblem<TSolution>,
        IRandomSource,
        TSolution> _factory;

    public DelegateThresholdAcceptingInitialSolutionGenerator(
        Func<
            IOptimizationProblem<TSolution>,
            IRandomSource,
            TSolution> factory)
    {
        _factory =
            factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(random);

        return
            _factory(
                problem,
                random);
    }
}