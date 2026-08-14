using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class LShadeOptimizerTests
{
    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            Run(
                seed: 20260814UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential);

        OptimizationResult<double[]> second =
            Run(
                seed: 20260814UL,
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
    public void SequentialAndParallelAreDeterministicallyEquivalent()
    {
        OptimizationResult<double[]> sequential =
            Run(
                seed: 987654321UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential);

        OptimizationResult<double[]> parallel =
            Run(
                seed: 987654321UL,
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
    public void LShadeImprovesSphereInDeterministicRun()
    {
        OptimizationResult<double[]> result =
            new LShadeOptimizer()
                .Optimize(
                    CreateSphereProblem(),
                    new LShadeParameters
                    {
                        InitialPopulationSize = 64,
                        MaximumFunctionEvaluations = 10_000,
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
                    new MaxIterationsStoppingCriterion(50),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    },
                    cancellationToken:
                        TestContext.Current.CancellationToken);

        Assert.True(
            result.BestFitness < 1.0,
            $"Expected L-SHADE to make substantial progress on Sphere; " +
            $"final={result.BestFitness:R}.");
    }

    [Fact]
    public void DescriptorContainsCanonicalReferenceAndVariablePopulationModel()
    {
        var descriptor =
            new LShadeOptimizer()
                .Descriptor;

        Assert.Equal(
            "L-SHADE",
            descriptor.Acronym);

        Assert.Equal(
            MetaheuristicsPlatform.Classification.MetaheuristicSolutionModel.VariablePopulation,
            descriptor.SolutionModel);

        Assert.Contains(
            descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/CEC.2014.6900380");
    }

    private static OptimizationResult<double[]> Run(
        ulong seed,
        DeExecutionMode variationMode,
        EvaluationExecutionMode evaluationMode)
    {
        return new LShadeOptimizer()
            .Optimize(
                CreateSphereProblem(),
                new LShadeParameters
                {
                    InitialPopulationSize = 64,
                    MaximumFunctionEvaluations = 10_000,
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