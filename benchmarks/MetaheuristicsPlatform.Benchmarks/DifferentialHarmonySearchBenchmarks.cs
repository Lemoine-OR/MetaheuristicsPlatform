using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class DifferentialHarmonySearchBenchmarks
{
    private readonly ContinuousOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(10, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private readonly ArraySolutionCloner<double> _cloner = new();
    private readonly DifferentialHarmonySearchParameters _parameters = new DifferentialHarmonySearchParameters { HarmonyMemorySize = 10, MaximumImprovisations = 25 };

    [Benchmark]
    public double DifferentialHarmonySearch() =>
        new DifferentialHarmonySearchOptimizer()
            .Optimize(
                _problem,
                _parameters,
                _cloner,
                new MaxEvaluationsStoppingCriterion(500),
                new OptimizationOptions { Seed = 123456UL })
            .BestFitness;

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * x[i];
        }
        return sum;
    }
}
