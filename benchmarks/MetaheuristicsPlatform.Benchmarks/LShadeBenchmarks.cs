using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class LShadeBenchmarks
{
    private ContinuousOptimizationProblem _problem = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
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
                supportsParallelEvaluation: true);
    }

    [Benchmark(Baseline = true)]
    public double ShadeFixedPopulation()
    {
        OptimizationResult<double[]> result =
            new ShadeOptimizer()
                .Optimize(
                    _problem,
                    new ShadeParameters
                    {
                        PopulationSize =
                            18 * Dimension,
                        MemorySize = 6,
                        PBestFraction = 0.11
                    },
                    new ArraySolutionCloner<double>(),
                    new MaxIterationsStoppingCriterion(20),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    });

        return result.BestFitness;
    }

    [Benchmark]
    public double LShadeLinearPopulation()
    {
        OptimizationResult<double[]> result =
            new LShadeOptimizer()
                .Optimize(
                    _problem,
                    new LShadeParameters
                    {
                        MaximumFunctionEvaluations =
                            10_000L * Dimension
                    },
                    new ArraySolutionCloner<double>(),
                    new MaxIterationsStoppingCriterion(20),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    });

        return result.BestFitness;
    }
}