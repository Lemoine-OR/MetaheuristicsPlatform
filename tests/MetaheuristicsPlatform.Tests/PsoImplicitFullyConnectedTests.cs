using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoImplicitFullyConnectedTests
{
    [Fact]
    public void GraphlessSocialContext_RejectsExplicitGraphAccess()
    {
        var context =
            new PsoSocialContext(
                new double[8],
                new double[8],
                new double[2],
                swarmSize: 2,
                dimension: 4,
                OptimizationSense.Minimize);

        Assert.False(context.HasGraph);

        Assert.Throws<InvalidOperationException>(
            () => _ = context.Graph);
    }

    [Fact]
    public void FullyConnectedCanonical_SequentialAndParallelRemainEquivalent()
    {
        OptimizationResult<double[]> sequential =
            Run(PsoExecutionMode.Sequential);

        OptimizationResult<double[]> parallel =
            Run(PsoExecutionMode.Parallel);

        Assert.Equal(
            sequential.BestFitness,
            parallel.BestFitness);

        Assert.Equal(
            sequential.BestSolution,
            parallel.BestSolution);
    }

    private static OptimizationResult<double[]> Run(
        PsoExecutionMode mode)
    {
        var problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    16,
                    -5.12,
                    5.12),
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

        return new ParticleSwarmOptimizer().Optimize(
            problem,
            new PsoParameters
            {
                SwarmSize = 64,
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
                        Mode = mode,
                        MaxDegreeOfParallelism = 4,
                        MinimumParallelWork = 0
                    }
            },
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(15),
            new OptimizationOptions
            {
                Seed = 20260814UL
            },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }
}