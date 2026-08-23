using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class SelfAdaptiveGlobalBestHarmonySearchBenchmarks
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

    private readonly SelfAdaptiveGlobalBestHarmonySearchParameters _parameters =
        new()
        {
            HarmonyMemorySize = 5,
            MaximumImprovisations = 200,
            InitialMeanHarmonyMemoryConsiderationRate = 0.98,
            InitialMeanPitchAdjustmentRate = 0.9,
            LearningPeriod = 100,
            MinimumPitchAdjustmentBandwidth = 0.0005,
            MaximumPitchAdjustmentBandwidthFractionOfRange = 0.1
        };

    [Benchmark]
    public double SelfAdaptiveGlobalBestHarmonySearch() =>
        new SelfAdaptiveGlobalBestHarmonySearchOptimizer().Optimize(
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
