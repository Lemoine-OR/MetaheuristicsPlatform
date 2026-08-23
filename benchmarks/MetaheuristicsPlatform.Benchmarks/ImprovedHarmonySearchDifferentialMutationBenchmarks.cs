using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class ImprovedHarmonySearchDifferentialMutationBenchmarks
{
    private readonly ContinuousOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(10, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private readonly ArraySolutionCloner<double> _cloner = new();
    private readonly ImprovedHarmonySearchDifferentialMutationParameters _parameters = new ImprovedHarmonySearchDifferentialMutationParameters { HarmonyMemorySize = 10, MaximumImprovisations = 25 };

    [Benchmark]
    public double ImprovedHarmonySearchDifferentialMutation() =>
        new ImprovedHarmonySearchDifferentialMutationOptimizer()
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
