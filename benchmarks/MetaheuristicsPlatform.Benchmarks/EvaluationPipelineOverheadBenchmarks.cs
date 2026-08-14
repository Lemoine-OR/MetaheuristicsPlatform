using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Evaluation.Instrumentation;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class EvaluationPipelineOverheadBenchmarks
{
    private readonly EvaluationPipeline<int, Box> _plain;
    private readonly EvaluationPipeline<int, Box> _instrumented;

    public EvaluationPipelineOverheadBenchmarks()
    {
        var characteristics =
            new EvaluationCharacteristics(
                SupportsParallelEvaluation: false,
                CostHint: EvaluationCostHint.Trivial,
                VariabilityHint: EvaluationVariabilityHint.Uniform);

        _plain =
            new EvaluationPipeline<int, Box>(
                new DelegateSolutionDecoder<int, Box>(
                    static (candidate, _) =>
                        new Box(candidate)),
                new DelegateSolutionEvaluator<Box>(
                    static (solution, _) =>
                        solution.Value * solution.Value),
                characteristics);

        _instrumented =
            new EvaluationPipeline<int, Box>(
                new DelegateSolutionDecoder<int, Box>(
                    static (candidate, _) =>
                        new Box(candidate)),
                new DelegateSolutionEvaluator<Box>(
                    static (solution, _) =>
                        solution.Value * solution.Value),
                characteristics,
                metricsSink:
                    new EvaluationPipelineMetrics());
    }

    [Benchmark(Baseline = true)]
    public double PlainPipeline()
    {
        int candidate = 17;

        return _plain
            .Evaluate(
                ref candidate)
            .Fitness;
    }

    [Benchmark]
    public double InstrumentedPipeline()
    {
        int candidate = 17;

        return _instrumented
            .Evaluate(
                ref candidate)
            .Fitness;
    }

    private sealed class Box
    {
        internal Box(int value)
        {
            Value = value;
        }

        internal int Value { get; }
    }
}