using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.DE;
using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class JdeBenchmarks
{
    private ContinuousOptimizationProblem _problem = null!;

    [Params(64, 256)]
    public int PopulationSize { get; set; }

    [Params(32, 128)]
    public int Dimension { get; set; }

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
                supportsParallelEvaluation: true,
                evaluationCostHint:
                    EvaluationCostHint.Light,
                evaluationVariabilityHint:
                    EvaluationVariabilityHint.Uniform);
    }

    [Benchmark(Baseline = true)]
    public double ClassicalDe()
    {
        OptimizationResult<double[]> result =
            new DifferentialEvolutionOptimizer()
                .Optimize(
                    _problem,
                    new DifferentialEvolutionParameters
                    {
                        PopulationSize =
                            PopulationSize,
                        DifferentialWeight = 0.5,
                        CrossoverProbability = 0.9,
                        MutationStrategy =
                            DeMutationStrategy.Rand1,
                        CrossoverStrategy =
                            DeCrossoverStrategy.Binomial,
                        BoundaryHandling =
                            DeBoundaryHandling.Clamp
                    },
                    new ArraySolutionCloner<double>(),
                    new MaxIterationsStoppingCriterion(30),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    });

        return result.BestFitness;
    }

    [Benchmark]
    public double Jde()
    {
        OptimizationResult<double[]> result =
            new SelfAdaptiveDifferentialEvolutionOptimizer()
                .Optimize(
                    _problem,
                    new JdeParameters
                    {
                        PopulationSize =
                            PopulationSize
                    },
                    new ArraySolutionCloner<double>(),
                    new MaxIterationsStoppingCriterion(30),
                    new OptimizationOptions
                    {
                        Seed = 20260814UL
                    });

        return result.BestFitness;
    }
}