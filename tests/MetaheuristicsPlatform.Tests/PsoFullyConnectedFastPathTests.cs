using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoFullyConnectedFastPathTests
{
    [Fact]
    public void FullyConnectedCanonicalFastPath_IsDeterministic()
    {
        OptimizationResult<double[]> first = Run();
        OptimizationResult<double[]> second = Run();

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
    }

    private static OptimizationResult<double[]> Run()
    {
        var problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    4,
                    -5.0,
                    5.0),
                OptimizationSense.Minimize,
                static position =>
                {
                    double sum = 0.0;

                    for (int i = 0;
                         i < position.Length;
                         i++)
                    {
                        sum +=
                            position[i] *
                            position[i];
                    }

                    return sum;
                },
                supportsParallelEvaluation: true);

        var optimizer =
            new ParticleSwarmOptimizer();

        return optimizer.Optimize(
            problem,
            new PsoParameters
            {
                SwarmSize = 32,
                Topology =
                    new FullyConnectedTopology(),
                InfluencePolicy =
                    new CanonicalBestInfluencePolicy(
                        2.05,
                        2.05),
                VelocityDynamics =
                    new ClercKennedyConstrictionDynamics(
                        4.10),
                Execution =
                    new PsoExecutionOptions
                    {
                        Mode =
                            PsoExecutionMode.Parallel,
                        MaxDegreeOfParallelism = 4,
                        MinimumParallelWork = 0
                    }
            },
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(10),
            new OptimizationOptions
            {
                Seed = 987654321UL
            },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }
}