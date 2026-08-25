using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.WhaleOptimization;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class WOAScientificBenchmarks
{
    private readonly ContinuousOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(30, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private readonly ArraySolutionCloner<double> _cloner = new();
    private readonly WhaleOptimizationAlgorithmParameters _parameters =
        new() { PopulationSize = 20, MaximumIterations = 10 };

    [Benchmark]
    public double Optimize() =>
        new WhaleOptimizationAlgorithmOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(5000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
        return sum;
    }
}
