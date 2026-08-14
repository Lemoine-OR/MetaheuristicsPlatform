using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.DE;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class DeVariationCrossoverBenchmarks
{
    private readonly DifferentialEvolutionOptimizer _optimizer =
        new();

    private ContinuousOptimizationProblem _problem = null!;

    [Params(16, 24, 32, 40, 48, 56, 64, 80)]
    public int PopulationSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem =
            CreateProblem(
                dimension: 32);
    }

    [Benchmark(Baseline = true)]
    public double SequentialVariation() =>
        Run(
            DeExecutionMode.Sequential);

    [Benchmark]
    public double ParallelVariation() =>
        Run(
            DeExecutionMode.Parallel);

    private double Run(
        DeExecutionMode variationMode)
    {
        OptimizationResult<double[]> result =
            _optimizer.Optimize(
                _problem,
                new DifferentialEvolutionParameters
                {
                    PopulationSize =
                        PopulationSize,
                    DifferentialWeight = 0.7,
                    CrossoverProbability = 0.9,
                    MutationStrategy =
                        DeMutationStrategy.Rand1,
                    CrossoverStrategy =
                        DeCrossoverStrategy.Binomial,
                    BoundaryHandling =
                        DeBoundaryHandling.Reflect,
                    VariationExecution =
                        new DeExecutionOptions
                        {
                            Mode =
                                variationMode
                        },
                    EvaluationExecution =
                        new EvaluationExecutionOptions
                        {
                            Mode =
                                EvaluationExecutionMode.Sequential
                        }
                },
                new ArraySolutionCloner<double>(),
                new MaxIterationsStoppingCriterion(30),
                new OptimizationOptions
                {
                    Seed = 20260814UL
                });

        return result.BestFitness;
    }

    private static ContinuousOptimizationProblem CreateProblem(
        int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
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