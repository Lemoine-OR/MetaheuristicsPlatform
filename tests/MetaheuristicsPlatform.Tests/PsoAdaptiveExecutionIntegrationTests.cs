using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using EvaluationParallelMode = MetaheuristicsPlatform.Execution.EvaluationExecutionMode;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoAdaptiveExecutionIntegrationTests
{
    [Fact]
    public void AutoAndForcedSequential_RemainDeterministicallyEquivalent()
    {
        OptimizationResult<double[]> sequential =
            Run(
                PsoExecutionMode.Sequential,
                EvaluationParallelMode.Sequential);

        OptimizationResult<double[]> auto =
            Run(
                PsoExecutionMode.Auto,
                EvaluationParallelMode.Auto);

        Assert.Equal(
            sequential.BestFitness,
            auto.BestFitness);

        Assert.Equal(
            sequential.BestSolution,
            auto.BestSolution);
    }

    private static OptimizationResult<double[]> Run(
        PsoExecutionMode movementMode,
        EvaluationParallelMode evaluationMode)
    {
        var problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    32,
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
                        double x = position[i];

                        sum +=
                            x * x +
                            10.0 *
                            (1.0 -
                             Math.Cos(
                                 2.0 *
                                 Math.PI *
                                 x));
                    }

                    return sum;
                },
                supportsParallelEvaluation: true,
                evaluationCostHint:
                    EvaluationCostHint.Medium,
                evaluationVariabilityHint:
                    EvaluationVariabilityHint.Uniform);

        return new ParticleSwarmOptimizer().Optimize(
            problem,
            new PsoParameters
            {
                SwarmSize = 80,
                Topology =
                    new FullyConnectedTopology(),
                InfluencePolicy =
                    new CanonicalBestInfluencePolicy(
                        2.05,
                        2.05),
                VelocityDynamics =
                    new ClercKennedyConstrictionDynamics(
                        4.10),
                MovementExecution =
                    new PsoExecutionOptions
                    {
                        Mode = movementMode
                    },
                EvaluationExecution =
                    new EvaluationExecutionOptions
                    {
                        Mode = evaluationMode
                    }
            },
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(10),
            new OptimizationOptions
            {
                Seed = 20260814UL
            },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }
}