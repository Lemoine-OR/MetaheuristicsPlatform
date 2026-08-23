using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class AdvancedParameterSettingFreeHarmonySearchIterationBenchmarks
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

    private readonly AdvancedParameterSettingFreeHarmonySearchIterationParameters
        _parameters =
        new()
        {
            HarmonyMemorySize = 50,
            MaximumImprovisations = 500,
            PitchAdjustmentBandwidthFractionOfRange = 0.001
        };

    [Benchmark]
    public double AdvancedParameterSettingFreeHarmonySearchIteration() =>
        new AdvancedParameterSettingFreeHarmonySearchIterationOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(1000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0;
             i < x.Length;
             i++)
        {
            sum +=
                x[i] *
                x[i];
        }

        return sum;
    }
}
