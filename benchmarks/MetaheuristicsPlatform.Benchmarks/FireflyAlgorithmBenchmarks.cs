using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Firefly;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class FireflyAlgorithmBenchmarks
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

    private readonly FireflyParameters _parameters =
        new()
        {
            PopulationSize = 20,
            MaximumIterations = 20,
            BaseAttractiveness = 1.0,
            LightAbsorptionCoefficient = 0.5,
            RandomizationAmplitude = 0.2
        };

    [Benchmark]
    public double FireflyAlgorithm() =>
        new FireflyOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(1000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            sum +=
                x[i] *
                x[i];
        }

        return sum;
    }
}
