using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.PSO.Speciation;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class SpeciesBasedPsoScientificBenchmarks
{
    private readonly ContinuousOptimizationProblem _problem =
        new(
            BoundedContinuousSearchSpace.Uniform(30, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private readonly ArraySolutionCloner<double> _cloner = new();
    private readonly SpeciesBasedPsoParameters _parameters = new SpeciesBasedPsoParameters { SwarmSize = 20, MaximumIterations = 10, SpeciesRadiusFraction = 0.1 };

    [Benchmark]
    public double Optimize() =>
        new SpeciesBasedParticleSwarmOptimizer().Optimize(
            _problem,
            _parameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
        return sum;
    }
}
