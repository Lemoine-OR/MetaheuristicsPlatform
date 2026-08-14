using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.DE;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class DeVariationShapeSensitivityBenchmarks
{
    private readonly DifferentialEvolutionOptimizer _optimizer =
        new();

    private ContinuousOptimizationProblem _problem = null!;

    [Params("16x128", "32x64", "64x32", "128x16")]
    public string Shape { get; set; } = null!;

    private int PopulationSize { get; set; }

    private int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string[] parts =
            Shape.Split('x');

        PopulationSize =
            int.Parse(
                parts[0],
                System.Globalization.CultureInfo.InvariantCulture);

        Dimension =
            int.Parse(
                parts[1],
                System.Globalization.CultureInfo.InvariantCulture);

        _problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    Dimension,
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
}