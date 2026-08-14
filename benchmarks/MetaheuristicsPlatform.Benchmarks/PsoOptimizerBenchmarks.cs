using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class PsoOptimizerBenchmarks
{
    private ContinuousOptimizationProblem _problem = null!;
    private ParticleSwarmOptimizer _optimizer = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [Params(40, 256)]
    public int SwarmSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem =
            new ContinuousOptimizationProblem(
                BoundedContinuousSearchSpace.Uniform(
                    Dimension,
                    -5.12,
                    5.12),
                OptimizationSense.Minimize,
                static position =>
                {
                    double sum = 0.0;

                    for (int i = 0;
                         i < position.Length;
                         i++)
                    {
                        sum +=
                            position[i] *
                            position[i];
                    }

                    return sum;
                },
                supportsParallelEvaluation: true);

        _optimizer =
            new ParticleSwarmOptimizer();
    }

    [Benchmark(Baseline = true)]
    public OptimizationResult<double[]> SequentialCanonical() =>
        Run(PsoExecutionMode.Sequential);

    [Benchmark]
    public OptimizationResult<double[]> ParallelCanonical() =>
        Run(PsoExecutionMode.Parallel);

    private OptimizationResult<double[]> Run(
        PsoExecutionMode mode)
    {
        var parameters =
            new PsoParameters
            {
                SwarmSize = SwarmSize,
                Topology =
                    new RingTopology(),
                InfluencePolicy =
                    new CanonicalBestInfluencePolicy(
                        2.05,
                        2.05),
                VelocityDynamics =
                    new ClercKennedyConstrictionDynamics(
                        4.10),
                Execution =
                    new PsoExecutionOptions
                    {
                        Mode = mode,
                        MinimumParallelWork = 0
                    }
            };

        return _optimizer.Optimize(
            _problem,
            parameters,
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(50),
            new OptimizationOptions
            {
                Seed = 123UL,
                CallbackEvents =
                    MetaheuristicsPlatform.Callbacks
                        .OptimizationCallbackEvents.None
            });
    }
}