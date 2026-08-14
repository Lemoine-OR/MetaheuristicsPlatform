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

[MemoryDiagnoser]
public class PsoParallelCrossoverBenchmarks
{
    private ParticleSwarmOptimizer _optimizer = null!;
    private ContinuousOptimizationProblem _problem = null!;
    private OptimizationOptions _runtime = null!;
    private IStoppingCriterion _stopping = null!;
    private PsoParameters _sequential = null!;
    private PsoParameters _parallel = null!;

    [Params(48, 56, 64, 80, 96, 112, 128)]
    public int SwarmSize { get; set; }

    public int Dimension => 32;

    [GlobalSetup]
    public void Setup()
    {
        _optimizer = new ParticleSwarmOptimizer();

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

                    for (int i = 0; i < position.Length; i++)
                    {
                        sum += position[i] * position[i];
                    }

                    return sum;
                },
                supportsParallelEvaluation: true);

        _sequential = Create(PsoExecutionMode.Sequential);
        _parallel = Create(PsoExecutionMode.Parallel);

        _runtime = new OptimizationOptions
        {
            Seed = 0x13579BDF2468ACE0UL,
            CallbackEvents = OptimizationCallbackEvents.None
        };

        _stopping = new MaxIterationsStoppingCriterion(30);
    }

    [Benchmark(Baseline = true)]
    public OptimizationResult<double[]> Sequential() =>
        Run(_sequential);

    [Benchmark]
    public OptimizationResult<double[]> Parallel() =>
        Run(_parallel);

    private PsoParameters Create(PsoExecutionMode mode) =>
        new()
        {
            SwarmSize = SwarmSize,
            Topology = new FullyConnectedTopology(),
            InfluencePolicy =
                new CanonicalBestInfluencePolicy(2.05, 2.05),
            VelocityDynamics =
                new ClercKennedyConstrictionDynamics(4.10),
            BoundaryHandling = PsoBoundaryHandling.Clamp,
            VelocityLimitRangeFraction = 1.0,
            EnableParallelObjectiveEvaluation = true,
            Execution = new PsoExecutionOptions
            {
                Mode = mode,
                MinimumParallelWork = 0
            }
        };

    private OptimizationResult<double[]> Run(PsoParameters parameters) =>
        _optimizer.Optimize(
            _problem,
            parameters,
            new ArraySolutionCloner<double>(),
            _stopping,
            _runtime);
}