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
/// End-to-end calibration for the particle-level parallelism crossover point.
/// Workload is encoded as swarmSize x dimension.
/// </summary>
[MemoryDiagnoser]
public class PsoParallelCalibrationBenchmarks
{
    private ParticleSwarmOptimizer _optimizer = null!;
    private ContinuousOptimizationProblem _problem = null!;
    private PsoParameters _sequential = null!;
    private PsoParameters _parallel = null!;
    private OptimizationOptions _runtime = null!;
    private IStoppingCriterion _stopping = null!;
    private int _swarmSize;
    private int _dimension;

    [Params(
        "32x8",
        "40x32",
        "64x64",
        "128x64",
        "256x64",
        "256x256")]
    public string Workload { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        string[] parts = Workload.Split('x');
        _swarmSize = int.Parse(parts[0]);
        _dimension = int.Parse(parts[1]);

        _optimizer = new ParticleSwarmOptimizer();

        _problem = new ContinuousOptimizationProblem(
            BoundedContinuousSearchSpace.Uniform(
                _dimension,
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

        _sequential = CreateParameters(
            PsoExecutionMode.Sequential);

        _parallel = CreateParameters(
            PsoExecutionMode.Parallel);

        _runtime = new OptimizationOptions
        {
            Seed = 0x0123456789ABCDEFUL,
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

    private PsoParameters CreateParameters(
        PsoExecutionMode mode) =>
        new()
        {
            SwarmSize = _swarmSize,
            Topology = new FullyConnectedTopology(),
            InfluencePolicy =
                new CanonicalBestInfluencePolicy(
                    2.05,
                    2.05),
            VelocityDynamics =
                new ClercKennedyConstrictionDynamics(4.10),
            BoundaryHandling =
                PsoBoundaryHandling.Clamp,
            VelocityLimitRangeFraction = 1.0,
            EnableParallelObjectiveEvaluation = true,
            Execution = new PsoExecutionOptions
            {
                Mode = mode,
                MinimumParallelWork = 0
            }
        };

    private OptimizationResult<double[]> Run(
        PsoParameters parameters) =>
        _optimizer.Optimize(
            _problem,
            parameters,
            new ArraySolutionCloner<double>(),
            _stopping,
            _runtime);
}