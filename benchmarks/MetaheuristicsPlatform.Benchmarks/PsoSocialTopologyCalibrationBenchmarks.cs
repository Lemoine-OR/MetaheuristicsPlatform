using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

/// <summary>
/// Measures the performance cost of representative topology/influence combinations.
/// </summary>
[MemoryDiagnoser]
public class PsoSocialTopologyCalibrationBenchmarks
{
    private ParticleSwarmOptimizer _optimizer = null!;
    private ContinuousOptimizationProblem _problem = null!;
    private OptimizationOptions _runtime = null!;
    private IStoppingCriterion _stopping = null!;

    [GlobalSetup]
    public void Setup()
    {
        const int dimension = 64;

        _optimizer = new ParticleSwarmOptimizer();

        _problem = new ContinuousOptimizationProblem(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.12,
                5.12),
            OptimizationSense.Minimize,
            static position =>
            {
                double sum = 0.0;

                for (int i = 0; i < position.Length; i++)
                {
                    sum += position[i] * position[i];
                }

                return sum;
            },
            supportsParallelEvaluation: true);

        _runtime = new OptimizationOptions
        {
            Seed = 0xC0FFEEUL,
            CallbackEvents = OptimizationCallbackEvents.None
        };

        _stopping = new MaxIterationsStoppingCriterion(25);
    }

    [Benchmark(Baseline = true)]
    public OptimizationResult<double[]> CanonicalFullyConnected() =>
        Run(
            128,
            new FullyConnectedTopology(),
            new CanonicalBestInfluencePolicy(
                2.05,
                2.05));

    [Benchmark]
    public OptimizationResult<double[]> CanonicalRing() =>
        Run(
            128,
            new RingTopology(),
            new CanonicalBestInfluencePolicy(
                2.05,
                2.05));

    [Benchmark]
    public OptimizationResult<double[]> FipsFullyConnected() =>
        Run(
            128,
            new FullyConnectedTopology(),
            new FullyInformedInfluencePolicy(4.10));

    [Benchmark]
    public OptimizationResult<double[]> FipsRing() =>
        Run(
            128,
            new RingTopology(),
            new FullyInformedInfluencePolicy(4.10));

    [Benchmark]
    public OptimizationResult<double[]> DClusterFips() =>
        Run(
            110,
            new DClusterTopology(clusterSize: 10),
            new FullyInformedInfluencePolicy(4.10));

    private OptimizationResult<double[]> Run(
        int swarmSize,
        IPsoTopology topology,
        IPsoInfluencePolicy influence)
    {
        var parameters = new PsoParameters
        {
            SwarmSize = swarmSize,
            Topology = topology,
            InfluencePolicy = influence,
            VelocityDynamics =
                new ClercKennedyConstrictionDynamics(4.10),
            Execution = new PsoExecutionOptions
            {
                Mode = PsoExecutionMode.Parallel,
                MinimumParallelWork = 0
            },
            EnableParallelObjectiveEvaluation = true
        };

        return _optimizer.Optimize(
            _problem,
            parameters,
            new ArraySolutionCloner<double>(),
            _stopping,
            _runtime);
    }
}