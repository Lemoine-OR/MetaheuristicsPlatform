using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class ShadeOptimizerTests
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
    public void ShadeImprovesItsExactInitialPopulationOnSphere()
    {
        const int populationSize = 100;
        const ulong seed = 123456789UL;

        ContinuousOptimizationProblem problem =
            CreateSphereProblem();

        double initialBest =
            ComputeInitialBest(
                problem,
                populationSize,
                seed);

        OptimizationResult<double[]> result =
            new ShadeOptimizer()
                .Optimize(
                    problem,
                    new ShadeParameters
                    {
                        PopulationSize =
                            populationSize,
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
                        Seed = seed,
                        RandomSourceFactory =
                            Xoshiro256StarStarRandomSourceFactory.Instance
                    },
                    cancellationToken:
                        TestContext.Current.CancellationToken);

        Assert.True(
            result.BestFitness < initialBest,
            $"Expected SHADE to improve its exact initial population. " +
            $"Initial={initialBest:R}; Final={result.BestFitness:R}.");
    }

    [Fact]
    public void DescriptorContainsCanonicalReference()
    {
        var descriptor =
            new ShadeOptimizer()
                .Descriptor;

        Assert.Equal(
            "SHADE",
            descriptor.Acronym);

        Assert.True(
            descriptor.Mechanisms.HasFlag(
                MetaheuristicsPlatform.Classification.MetaheuristicMechanism.Adaptive));

        Assert.True(
            descriptor.Mechanisms.HasFlag(
                MetaheuristicsPlatform.Classification.MetaheuristicMechanism.MemoryBased));

        Assert.Contains(
            descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/CEC.2013.6557555");
    }

    private static OptimizationResult<double[]> Run(
        ulong seed,
        DeExecutionMode variationMode,
        EvaluationExecutionMode evaluationMode)
    {
        return new ShadeOptimizer()
            .Optimize(
                CreateSphereProblem(),
                new ShadeParameters
                {
                    PopulationSize = 64,
                    MemorySize = 64,
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

    private static double ComputeInitialBest(
        ContinuousOptimizationProblem problem,
        int populationSize,
        ulong seed)
    {
        var streams =
            new DeTargetRandomStreams(
                populationSize,
                seed,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        double[] candidate =
            new double[
                problem.SearchSpace.Dimension];

        double best =
            double.PositiveInfinity;

        for (int target = 0;
             target < populationSize;
             target++)
        {
            problem.SearchSpace.Sample(
                streams.Get(target),
                candidate);

            double fitness =
                problem.Evaluate(
                    candidate);

            if (fitness < best)
            {
                best = fitness;
            }
        }

        return best;
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