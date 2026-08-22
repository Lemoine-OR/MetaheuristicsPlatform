using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.CMAES;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class AdvancedCmaEsBenchmarks
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

    private readonly CmaEsParameters _parameters =
        new()
        {
            PopulationSize = 20,
            ParentCount = 10,
            MaximumGenerations = 15,
            InitialStepSize = 1.0
        };

    [Benchmark]
    public double ActiveCmaEs() =>
        new ActiveCmaEsOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(300),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    [Benchmark]
    public double SeparableCmaEs() =>
        new SeparableCmaEsOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(300),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * x[i];
        }

        return sum;
    }
}
