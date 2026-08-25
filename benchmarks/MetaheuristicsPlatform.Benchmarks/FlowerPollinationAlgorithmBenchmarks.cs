using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.FlowerPollination;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class FPAScientificBenchmarks
{
    private readonly ContinuousOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(
                30,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            Sphere);

    private readonly ArraySolutionCloner<double> _cloner =
        new();

    private readonly FlowerPollinationParameters _parameters =
        new() { PopulationSize = 20, MaximumIterations = 10 };

    [Benchmark]
    public double Optimize() =>
        new FlowerPollinationOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(2000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
            sum += x[i] * x[i];

        return sum;
    }
}
