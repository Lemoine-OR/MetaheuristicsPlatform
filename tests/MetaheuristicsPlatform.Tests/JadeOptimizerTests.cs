using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class JadeOptimizerTests
{
    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            Run(
                seed: 20260814UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential,
                useArchive: true);

        OptimizationResult<double[]> second =
            Run(
                seed: 20260814UL,
                DeExecutionMode.Sequential,
                EvaluationExecutionMode.Sequential,
                useArchive: true);

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
                EvaluationExecutionMode.Sequential,
                useArchive: true);

        OptimizationResult<double[]> parallel =
            Run(
                seed: 987654321UL,
                DeExecutionMode.Parallel,
                EvaluationExecutionMode.Parallel,
                useArchive: true);

        Assert.Equal(
            sequential.BestFitness,
            parallel.BestFitness);

        Assert.Equal(
            sequential.BestSolution,
            parallel.BestSolution);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OptionalArchiveModesRunAndImproveInitialPopulation(
        bool useArchive)
    {
        const int populationSize = 64;
        const ulong seed = 123456UL;

        ContinuousOptimizationProblem problem =
            CreateSphereProblem();

        double initialBest =
            ComputeInitialBest(
                problem,
                populationSize,
                seed);

        OptimizationResult<double[]> result =
            new JadeOptimizer()
                .Optimize(
                    problem,
                    new JadeParameters
                    {
                        PopulationSize = populationSize,
                        UseExternalArchive = useArchive,
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
            $"Expected JADE to improve its exact initial population. " +
            $"Initial={initialBest:R}; Final={result.BestFitness:R}.");
    }

    [Fact]
    public void DescriptorContainsCanonicalReferenceAndAdaptiveMemoryMechanisms()
    {
        var descriptor =
            new JadeOptimizer()
                .Descriptor;

        Assert.Equal(
            "JADE",
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
                "10.1109/TEVC.2009.2014613");
    }

    private static OptimizationResult<double[]> Run(
        ulong seed,
        DeExecutionMode variationMode,
        EvaluationExecutionMode evaluationMode,
        bool useArchive)
    {
        return new JadeOptimizer()
            .Optimize(
                CreateSphereProblem(),
                new JadeParameters
                {
                    PopulationSize = 64,
                    UseExternalArchive = useArchive,
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
            new MetaheuristicsPlatform.Algorithms.DE.Random.DeTargetRandomStreams(
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