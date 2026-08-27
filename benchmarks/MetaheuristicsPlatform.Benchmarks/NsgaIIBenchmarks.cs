using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaII;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class NsgaIIBenchmarks
{
    private readonly ContinuousMultiobjectiveOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(6, 0.0, 1.0),
            new[] { OptimizationSense.Minimize, OptimizationSense.Minimize },
            static (ReadOnlySpan<double> x, Span<double> f) =>
            {
                f[0] = x[0];
                double sum = 0.0;
                for (int i = 1; i < x.Length; i++) sum += x[i];
                double g = 1.0 + 9.0 * sum / (x.Length - 1);
                f[1] = g * (1.0 - Math.Sqrt(x[0] / g));
            });

    [Benchmark]
    public int Optimize()
    {
        return new NsgaIIOptimizer()
            .Optimize(
                _problem,
                new NsgaIIParameters { MaximumGenerations = 5 },
                new OptimizationOptions { Seed = 123456UL })
            .ParetoFront.Count;
    }
}
