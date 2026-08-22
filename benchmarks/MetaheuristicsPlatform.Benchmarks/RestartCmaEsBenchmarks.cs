using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.CMAES;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class RestartCmaEsBenchmarks
{
    private readonly ContinuousOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(
                20,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            Sphere);

    private readonly ArraySolutionCloner<double> _cloner =
        new();

    private readonly RestartCmaEsParameters _parameters =
        new()
        {
            InitialPopulationSize = 10,
            MaximumRestarts = 2,
            MaximumGenerationsPerRestart = 5,
            InitialStepSize = 1.0
        };

    [Benchmark]
    public double IpopCmaEs() =>
        new IpopCmaEsOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(350),
            new OptimizationOptions { Seed = 123UL }).BestFitness;

    [Benchmark]
    public double BipopCmaEs() =>
        new BipopCmaEsOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(350),
            new OptimizationOptions { Seed = 123UL }).BestFitness;

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
