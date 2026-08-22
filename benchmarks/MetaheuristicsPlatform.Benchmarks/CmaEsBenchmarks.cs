using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.CMAES;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class CmaEsBenchmarks
{
    private readonly CmaEsOptimizer _optimizer = new();

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

    [Benchmark]
    public double CanonicalFullCovarianceCmaEs()
    {
        OptimizationResult<double[]> result =
            _optimizer.Optimize(
                _problem,
                new CmaEsParameters
                {
                    PopulationSize = 16,
                    ParentCount = 8,
                    MaximumGenerations = 20,
                    InitialStepSize = 1.0
                },
                _cloner,
                new MaxEvaluationsStoppingCriterion(320),
                new OptimizationOptions { Seed = 123456UL });

        return result.BestFitness;
    }

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
