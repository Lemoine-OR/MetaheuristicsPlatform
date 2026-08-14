using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class ShadeBenchmarks
{
    private ContinuousOptimizationProblem _problem = null!;

    [Params(64, 256)]
    public int PopulationSize { get; set; }

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
    public double Jade()
    {
        OptimizationResult<double[]> result =
            new JadeOptimizer()
                .Optimize(
                    _problem,
                    new JadeParameters
                    {
                        PopulationSize =
                            PopulationSize
                    },
                    new ArraySolutionCloner<double>(),
                    new MaxIterationsStoppingCriterion(30),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    });

        return result.BestFitness;
    }

    [Benchmark]
    public double Shade()
    {
        OptimizationResult<double[]> result =
            new ShadeOptimizer()
                .Optimize(
                    _problem,
                    new ShadeParameters
                    {
                        PopulationSize =
                            PopulationSize,
                        MemorySize =
                            100
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