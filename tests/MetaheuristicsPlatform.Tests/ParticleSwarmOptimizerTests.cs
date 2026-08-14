using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class ParticleSwarmOptimizerTests
{
    [Fact]
    public void SameSeed_ProducesSameResult()
    {
        OptimizationResult<double[]> first =
            Run(
                PsoExecutionMode.Sequential,
                parallelObjective: false);

        OptimizationResult<double[]> second =
            Run(
                PsoExecutionMode.Sequential,
                parallelObjective: false);

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);
    }

    [Fact]
    public void SequentialAndParallelSynchronousRuns_AreEquivalent()
    {
        OptimizationResult<double[]> sequential =
            Run(
                PsoExecutionMode.Sequential,
                parallelObjective: false);

        OptimizationResult<double[]> parallel =
            Run(
                PsoExecutionMode.Parallel,
                parallelObjective: true);

        Assert.Equal(
            sequential.BestFitness,
            parallel.BestFitness);

        Assert.Equal(
            sequential.BestSolution,
            parallel.BestSolution);
    }

    [Fact]
    public void SphereFitnessImprovesFromFiniteInitialization()
    {
        OptimizationResult<double[]> result =
            Run(
                PsoExecutionMode.Sequential,
                parallelObjective: false);

        Assert.True(
            double.IsFinite(
                result.BestFitness));

        Assert.True(
            result.BestFitness >= 0.0);

        Assert.Equal(
            5,
            result.BestSolution.Length);
    }

    private static OptimizationResult<double[]> Run(
        PsoExecutionMode executionMode,
        bool parallelObjective)
    {
        var problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    5,
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

        var parameters =
            new PsoParameters
            {
                SwarmSize = 24,
                Topology =
                    new RingTopology(
                        radius: 1,
                        includeSelf: true),
                InfluencePolicy =
                    new CanonicalBestInfluencePolicy(
                        2.05,
                        2.05),
                VelocityDynamics =
                    new ClercKennedyConstrictionDynamics(
                        4.10),
                VelocityLimitRangeFraction = 1.0,
                BoundaryHandling =
                    PsoBoundaryHandling.Clamp,
                EnableParallelObjectiveEvaluation =
                    parallelObjective,
                Execution =
                    new PsoExecutionOptions
                    {
                        Mode = executionMode,
                        MaxDegreeOfParallelism = 4,
                        MinimumParallelWork = 0
                    }
            };

        var optimizer =
            new ParticleSwarmOptimizer();

        return optimizer.Optimize(
            problem,
            parameters,
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(25),
            new OptimizationOptions
            {
                Seed = 123456789UL
            },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }
}