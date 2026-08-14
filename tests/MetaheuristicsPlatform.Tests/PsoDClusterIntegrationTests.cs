using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoDClusterIntegrationTests
{
    [Fact]
    public void ExactDCluster_RunsInsideOptimizer()
    {
        var problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    3,
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
                });

        var parameters =
            new PsoParameters
            {
                SwarmSize = 20,
                Topology =
                    new DClusterTopology(
                        clusterSize: 4),
                InfluencePolicy =
                    new FullyInformedInfluencePolicy(
                        4.10),
                VelocityDynamics =
                    new ClercKennedyConstrictionDynamics(
                        4.10),
                Execution =
                    new PsoExecutionOptions
                    {
                        Mode =
                            PsoExecutionMode.Sequential
                    }
            };

        var optimizer =
            new ParticleSwarmOptimizer();

        OptimizationResult<double[]> result =
            optimizer.Optimize(
                problem,
                parameters,
                new ArraySolutionCloner<double>(),
                new MaxIterationsStoppingCriterion(5),
                new OptimizationOptions
                {
                    Seed = 42UL
                },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.True(
            double.IsFinite(
                result.BestFitness));
    }
}