using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.AntColony;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class AdvancedAntColonyBenchmarks
{
    private readonly Model _model = new();
    private readonly Problem _problem = new();
    private readonly ImmutableSolutionCloner<int> _cloner = new();
    private readonly ConstantAntSystemDepositPolicy<int> _deposit = new(0.1);

    [Benchmark]
    public double AntColonySystem()
    {
        var optimizer =
            new AntColonySystemOptimizer<int,int,int,Enumerator>(
                _model,
                _deposit);

        return optimizer.Optimize(
            _problem,
            new AntColonySystemParameters
            {
                AntCount = 32,
                MaximumIterations = 5
            },
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;
    }

    [Benchmark]
    public double MaxMinAntSystem()
    {
        var optimizer =
            new MaxMinAntSystemOptimizer<int,int,int,Enumerator>(
                _model,
                _deposit);

        return optimizer.Optimize(
            _problem,
            new MaxMinAntSystemParameters
            {
                AntCount = 32,
                MaximumIterations = 5
            },
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;
    }

    private sealed class Problem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) => Math.Abs(100 - solution) + 1.0;
    }

    private struct Enumerator : IAntColonyCandidateEnumerator<int>
    {
        private int _i;
        public bool MoveNext(out int component)
        {
            _i++;
            if (_i <= 4)
            {
                component = _i;
                return true;
            }
            component = 0;
            return false;
        }
    }

    private sealed class Model :
        IAntColonyConstructionModel<int,int,int,Enumerator>
    {
        public int CreateInitialSolution(IOptimizationProblem<int> problem, IRandomSource random) => 0;
        public bool IsComplete(in int solution, IOptimizationProblem<int> problem) => solution >= 100;
        public Enumerator GetCandidateEnumerator(in int solution, IOptimizationProblem<int> problem) => new();
        public int GetPheromoneKey(in int solution, in int component, IOptimizationProblem<int> problem) => (solution * 10) + component;
        public double EvaluateHeuristic(in int solution, in int component, IOptimizationProblem<int> problem) => 1.0 / (1.0 + Math.Abs(100 - (solution + component)));
        public void ApplyComponent(ref int solution, in int component, IOptimizationProblem<int> problem) => solution += component;
    }
}
