using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.HyperHeuristics.LateAcceptanceHyperHeuristic;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class LateAcceptanceHyperHeuristicOptimizerBenchmarks
{
    private readonly BenchmarkDomain _domain = new();

    [Benchmark]
    public double Optimize()
    {
        return new LateAcceptanceHyperHeuristicOptimizer()
            .Optimize(
                _domain,
                new LateAcceptanceHyperHeuristicParameters
                {
                    MaximumIterations = 40
                },
                new OptimizationOptions
                {
                    Seed = 123456UL
                })
            .BestObjective;
    }

    private sealed class VectorSolution :
        IHyperHeuristicSolution
    {
        public VectorSolution(double value) { Value = value; }
        public double Value { get; set; }
        public IHyperHeuristicSolution Clone() => new VectorSolution(Value);
    }

    private sealed class ScaleHeuristic :
        ILowLevelHeuristic
    {
        public string Id => "scale-half";

        public void Apply(
            IHyperHeuristicSolution solution,
            IRandomSource random)
        {
            ((VectorSolution)solution).Value *= 0.5;
        }
    }

    private sealed class NudgeHeuristic :
        ILowLevelHeuristic
    {
        public string Id => "nudge-zero";

        public void Apply(
            IHyperHeuristicSolution solution,
            IRandomSource random)
        {
            VectorSolution vector = (VectorSolution)solution;

            vector.Value -=
                Math.Sign(vector.Value) *
                Math.Min(Math.Abs(vector.Value), 0.25);
        }
    }

    private sealed class BenchmarkDomain :
        IHyperHeuristicDomain
    {
        private readonly ILowLevelHeuristic[] _heuristics =
            new ILowLevelHeuristic[]
            {
                new ScaleHeuristic(),
                new NudgeHeuristic()
            };

        public OptimizationSense Sense => OptimizationSense.Minimize;
        public IReadOnlyList<ILowLevelHeuristic> Heuristics => _heuristics;

        public IHyperHeuristicSolution CreateInitial(
            IRandomSource random) =>
            new VectorSolution(8.0);

        public double Evaluate(
            IHyperHeuristicSolution solution)
        {
            VectorSolution vector = (VectorSolution)solution;
            return vector.Value * vector.Value;
        }

        public double[] Describe(
            IHyperHeuristicSolution solution)
        {
            VectorSolution vector = (VectorSolution)solution;
            return new[] { vector.Value, Math.Abs(vector.Value) };
        }
    }
}
