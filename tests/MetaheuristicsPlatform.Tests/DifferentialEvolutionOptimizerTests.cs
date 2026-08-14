using MetaheuristicsPlatform.Algorithms.DE;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class DifferentialEvolutionOptimizerTests
{
    [Fact]
    public void SameSeed_ProducesSameResult()
    {
        OptimizationResult<double[]> first =
            Run(
                987654321UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential,
                DeMutationStrategy.Rand1,
                DeCrossoverStrategy.Binomial);

        OptimizationResult<double[]> second =
            Run(
                987654321UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential,
                DeMutationStrategy.Rand1,
                DeCrossoverStrategy.Binomial);

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);
    }

    [Fact]
    public void SequentialAndParallelVariation_AreDeterministicallyEquivalent()
    {
        OptimizationResult<double[]> sequential =
            Run(
                123456UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential,
                DeMutationStrategy.Rand1,
                DeCrossoverStrategy.Binomial);

        OptimizationResult<double[]> parallel =
            Run(
                123456UL,
                DeExecutionMode.Parallel,
                EvaluationExecutionMode.Parallel,
                DeMutationStrategy.Rand1,
                DeCrossoverStrategy.Binomial);

        Assert.Equal(
            sequential.BestFitness,
            parallel.BestFitness);

        Assert.Equal(
            sequential.BestSolution,
            parallel.BestSolution);
    }

    [Theory]
    [InlineData(
        DeMutationStrategy.Rand1,
        DeCrossoverStrategy.Binomial)]
    [InlineData(
        DeMutationStrategy.Best1,
        DeCrossoverStrategy.Binomial)]
    [InlineData(
        DeMutationStrategy.CurrentToBest1,
        DeCrossoverStrategy.Binomial)]
    [InlineData(
        DeMutationStrategy.Rand2,
        DeCrossoverStrategy.Binomial)]
    [InlineData(
        DeMutationStrategy.Rand1,
        DeCrossoverStrategy.Exponential)]
    [InlineData(
        DeMutationStrategy.Best1,
        DeCrossoverStrategy.Exponential)]
    public void ClassicalStrategies_RunAndRemainInsideBounds(
        DeMutationStrategy mutation,
        DeCrossoverStrategy crossover)
    {
        OptimizationResult<double[]> result =
            Run(
                42UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential,
                mutation,
                crossover);

        Assert.All(
            result.BestSolution,
            value =>
                Assert.InRange(
                    value,
                    -5.12,
                    5.12));

        Assert.True(
            double.IsFinite(
                result.BestFitness));
    }

    [Fact]
    public void OptimizerImprovesSphereFromInitialPopulationInTypicalRun()
    {
        const int populationSize = 64;
        const ulong seed = 20260814UL;

        ContinuousOptimizationProblem problem =
            CreateSphereProblem();

        double initialBest =
            ComputeInitialPopulationBest(
                problem,
                populationSize,
                seed);

        var optimizer =
            new DifferentialEvolutionOptimizer();

        OptimizationResult<double[]> result =
            optimizer.Optimize(
                problem,
                new DifferentialEvolutionParameters
                {
                    PopulationSize = populationSize,
                    DifferentialWeight = 0.7,
                    CrossoverProbability = 0.9,
                    MutationStrategy =
                        DeMutationStrategy.Rand1,
                    CrossoverStrategy =
                        DeCrossoverStrategy.Binomial,
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
            $"Expected DE to improve the initial best. " +
            $"Initial={initialBest:R}, Final={result.BestFitness:R}.");
    }

    private static double ComputeInitialPopulationBest(
        ContinuousOptimizationProblem problem,
        int populationSize,
        ulong seed)
    {
        var streams =
            new DeTargetRandomStreams(
                populationSize,
                seed,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        double best =
            double.PositiveInfinity;

        double[] candidate =
            new double[
                problem.SearchSpace.Dimension];

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
                best =
                    fitness;
            }
        }

        return best;
    }

    private static OptimizationResult<double[]> Run(
        ulong seed,
        DeExecutionMode variationMode,
        EvaluationExecutionMode evaluationMode,
        DeMutationStrategy mutation,
        DeCrossoverStrategy crossover)
    {
        var optimizer =
            new DifferentialEvolutionOptimizer();

        return optimizer.Optimize(
            CreateSphereProblem(),
            new DifferentialEvolutionParameters
            {
                PopulationSize =
                    mutation == DeMutationStrategy.Rand2
                        ? 72
                        : 64,
                DifferentialWeight = 0.7,
                CrossoverProbability = 0.9,
                MutationStrategy = mutation,
                CrossoverStrategy = crossover,
                BoundaryHandling =
                    DeBoundaryHandling.Reflect,
                VariationExecution =
                    new DeExecutionOptions
                    {
                        Mode =
                            variationMode,
                        MaxDegreeOfParallelism = 4
                    },
                EvaluationExecution =
                    new EvaluationExecutionOptions
                    {
                        Mode =
                            evaluationMode,
                        MaxDegreeOfParallelism = 4
                    }
            },
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(20),
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