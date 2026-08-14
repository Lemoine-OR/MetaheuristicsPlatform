using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.PSO;
using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

/// <summary>
/// Measures how objective-function cost changes the value of parallel evaluation.
/// </summary>
[MemoryDiagnoser]
public class PsoObjectiveCostCalibrationBenchmarks
{
    private ParticleSwarmOptimizer _optimizer = null!;
    private ContinuousOptimizationProblem _cheapProblem = null!;
    private ContinuousOptimizationProblem _mediumProblem = null!;
    private PsoParameters _sequential = null!;
    private PsoParameters _parallel = null!;
    private OptimizationOptions _runtime = null!;
    private IStoppingCriterion _stopping = null!;

    [Params(64, 256)]
    public int SwarmSize { get; set; }

    [Params(32, 128)]
    public int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _optimizer = new ParticleSwarmOptimizer();

        IBoundedContinuousSearchSpace space =
            BoundedContinuousSearchSpace.Uniform(
                Dimension,
                -5.12,
                5.12);

        _cheapProblem = new ContinuousOptimizationProblem(
            space,
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
            supportsParallelEvaluation: true,
            evaluationCostHint:
                EvaluationCostHint.Trivial,
            evaluationVariabilityHint:
                EvaluationVariabilityHint.Uniform);

        _mediumProblem = new ContinuousOptimizationProblem(
            space,
            OptimizationSense.Minimize,
            static position =>
            {
                double sum = 0.0;

                for (int i = 0; i < position.Length; i++)
                {
                    double x = position[i];

                    // Deliberately more CPU-intensive but deterministic/pure.
                    double local = x * x;
                    local += 10.0 * (1.0 - Math.Cos(2.0 * Math.PI * x));
                    local += Math.Abs(Math.Sin(x)) * 0.01;
                    sum += local;
                }

                return sum;
            },
            supportsParallelEvaluation: true,
            evaluationCostHint:
                EvaluationCostHint.Medium,
            evaluationVariabilityHint:
                EvaluationVariabilityHint.Uniform);

        _sequential = CreateParameters(
            PsoExecutionMode.Sequential);

        _parallel = CreateParameters(
            PsoExecutionMode.Parallel);

        _runtime = new OptimizationOptions
        {
            Seed = 0x0BADF00DUL,
            CallbackEvents = OptimizationCallbackEvents.None
        };

        _stopping = new MaxIterationsStoppingCriterion(20);
    }

    [Benchmark(Baseline = true)]
    public OptimizationResult<double[]> CheapSequential() =>
        Run(_cheapProblem, _sequential);

    [Benchmark]
    public OptimizationResult<double[]> CheapParallel() =>
        Run(_cheapProblem, _parallel);

    [Benchmark]
    public OptimizationResult<double[]> MediumSequential() =>
        Run(_mediumProblem, _sequential);

    [Benchmark]
    public OptimizationResult<double[]> MediumParallel() =>
        Run(_mediumProblem, _parallel);

    private PsoParameters CreateParameters(
        PsoExecutionMode mode) =>
        new()
        {
            SwarmSize = SwarmSize,
            Topology = new FullyConnectedTopology(),
            InfluencePolicy =
                new CanonicalBestInfluencePolicy(
                    2.05,
                    2.05),
            VelocityDynamics =
                new ClercKennedyConstrictionDynamics(4.10),
            Execution = new PsoExecutionOptions
            {
                Mode = mode,
                MinimumParallelWork = 0
            },
            EnableParallelObjectiveEvaluation = true
        };

    private OptimizationResult<double[]> Run(
        ContinuousOptimizationProblem problem,
        PsoParameters parameters) =>
        _optimizer.Optimize(
            problem,
            parameters,
            new ArraySolutionCloner<double>(),
            _stopping,
            _runtime);
}