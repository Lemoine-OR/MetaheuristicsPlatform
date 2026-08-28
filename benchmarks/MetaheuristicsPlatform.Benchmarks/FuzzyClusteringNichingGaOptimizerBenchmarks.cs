using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Multimodal.FuzzyClusteringNichingGa;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class FuzzyClusteringNichingGaOptimizerBenchmarks
{
    private readonly ContinuousMultimodalOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(
                4,
                -1.0,
                1.0),
            OptimizationSense.Minimize,
            static x =>
                Math.Sin(3.0 * Math.PI * x[0]) *
                Math.Sin(3.0 * Math.PI * x[0]) +
                Math.Sin(3.0 * Math.PI * x[1]) *
                Math.Sin(3.0 * Math.PI * x[1]) +
                x[2] * x[2] +
                x[3] * x[3]);

    [Benchmark]
    public double Optimize()
    {
        return new FuzzyClusteringNichingGaOptimizer()
            .Optimize(
                _problem,
                new FuzzyClusteringNichingGaParameters { MaximumGenerations = 2, PopulationSize = 24, ClusterCount = 4 },
                new OptimizationOptions
                {
                    Seed = 123456UL
                })
            .Optima[0].Objective;
    }
}
