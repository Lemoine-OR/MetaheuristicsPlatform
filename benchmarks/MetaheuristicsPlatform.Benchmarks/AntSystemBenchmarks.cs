using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.AntColony;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class AntSystemBenchmarks
{
    private readonly AntSystemOptimizer<int, int, int, CandidateEnumerator>
        _optimizer =
        new(
            new ConstructionModel(),
            new ConstantAntSystemDepositPolicy<int>());

    private readonly Problem _problem = new();
    private readonly ImmutableSolutionCloner<int> _cloner = new();

    [Benchmark]
    public double CanonicalAntSystem()
    {
        return _optimizer.Optimize(
            _problem,
            new AntSystemParameters
            {
                AntCount = 32,
                MaximumIterations = 10,
                Alpha = 1.0,
                Beta = 2.0,
                EvaporationRate = 0.5
            },
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;
    }

    private sealed class Problem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Math.Abs(100 - solution) + 1.0;
    }

    private struct CandidateEnumerator :
        IAntColonyCandidateEnumerator<int>
    {
        private int _index;

        public bool MoveNext(out int component)
        {
            _index++;

            if (_index <= 4)
            {
                component = _index;
                return true;
            }

            component = default;
            return false;
        }
    }

    private sealed class ConstructionModel :
        IAntColonyConstructionModel<int, int, int, CandidateEnumerator>
    {
        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            solution >= 100;

        public CandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public int GetPheromoneKey(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            ((solution % 32) * 10) + component;

        public double EvaluateHeuristic(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            component;

        public void ApplyComponent(
            ref int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            solution += component;
    }
}
