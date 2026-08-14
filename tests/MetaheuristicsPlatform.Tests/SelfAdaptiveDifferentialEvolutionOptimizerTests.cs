using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class SelfAdaptiveDifferentialEvolutionOptimizerTests
{
    [Fact]
    public void SameSeed_ProducesSameResult()
    {
        OptimizationResult<double[]> first =
            Run(
                123456789UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential);

        OptimizationResult<double[]> second =
            Run(
                123456789UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential);

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);
    }

    [Fact]
    public void SequentialAndParallel_AreDeterministicallyEquivalent()
    {
        OptimizationResult<double[]> sequential =
            Run(
                987654321UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential);

        OptimizationResult<double[]> parallel =
            Run(
                987654321UL,
                DeExecutionMode.Parallel,
                EvaluationExecutionMode.Parallel);

        Assert.Equal(
            sequential.BestFitness,
            parallel.BestFitness);

        Assert.Equal(
            sequential.BestSolution,
            parallel.BestSolution);
    }

    [Fact]
    public void CanonicalJdeImprovesSphere()
    {
        OptimizationResult<double[]> result =
            new SelfAdaptiveDifferentialEvolutionOptimizer()
                .Optimize(
                    CreateSphereProblem(),
                    new JdeParameters
                    {
                        PopulationSize = 100,
                        VariationExecution =
                            new DeExecutionOptions
                            {
                                Mode =
                                    DeExecutionMode.Sequential
                            },
                        EvaluationExecution =
                            new EvaluationExecutionOptions
                            {
                                Mode =
                                    EvaluationExecutionMode.Sequential
                            }
                    },
                    new ArraySolutionCloner<double>(),
                    new MaxIterationsStoppingCriterion(100),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    },
                    cancellationToken:
                        TestContext.Current.CancellationToken);

        Assert.True(
            result.BestFitness < 1.0,
            $"Expected canonical jDE to make substantial progress on Sphere; final={result.BestFitness:R}.");
    }

    [Fact]
    public void Descriptor_ContainsBrestReferenceAndAdaptiveMechanism()
    {
        var descriptor =
            new SelfAdaptiveDifferentialEvolutionOptimizer()
                .Descriptor;

        Assert.Equal(
            "jDE",
            descriptor.Acronym);

        Assert.True(
            descriptor.Mechanisms.HasFlag(
                MetaheuristicsPlatform.Classification.MetaheuristicMechanism.Adaptive));

        Assert.Contains(
            descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/TEVC.2006.872133");
    }

    private static OptimizationResult<double[]> Run(
        ulong seed,
        DeExecutionMode variationMode,
        EvaluationExecutionMode evaluationMode)
    {
        return new SelfAdaptiveDifferentialEvolutionOptimizer()
            .Optimize(
                CreateSphereProblem(),
                new JdeParameters
                {
                    PopulationSize = 64,
                    VariationExecution =
                        new DeExecutionOptions
                        {
                            Mode = variationMode,
                            MaxDegreeOfParallelism = 4
                        },
                    EvaluationExecution =
                        new EvaluationExecutionOptions
                        {
                            Mode = evaluationMode,
                            MaxDegreeOfParallelism = 4
                        }
                },
                new ArraySolutionCloner<double>(),
                new MaxIterationsStoppingCriterion(30),
                new OptimizationOptions
                {
                    Seed = seed
                },
                cancellationToken:
                    TestContext.Current.CancellationToken);
    }

    private static ContinuousOptimizationProblem
        CreateSphereProblem() =>
        new(
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
            supportsParallelEvaluation: true,
            evaluationCostHint:
                EvaluationCostHint.Light,
            evaluationVariabilityHint:
                EvaluationVariabilityHint.Uniform);
}