using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class LargeNeighborhoodSearchBenchmarks
{
    private readonly LargeNeighborhoodSearchOptimizer<int,int> _optimizer =
        new(
            new ConstantInitial(),
            new SyntheticDestroy(),
            new SyntheticRepair());

    private readonly LargeNeighborhoodSearchParameters _parameters =
        new()
        {
            DestructionSize = 16,
            MaximumIterations = 500
        };

    [Benchmark]
    public double LargeNeighborhoodSearch() =>
        _optimizer.Optimize(
            new SyntheticProblem(),
            _parameters,
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(501),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;

    private sealed class SyntheticProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution) =>
            Math.Abs(
                solution);
    }

    private sealed class ConstantInitial :
        INeighborhoodSearchInitialSolutionGenerator<int>
    {
        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            1000;
    }

    private sealed class SyntheticDestroy :
        ILargeNeighborhoodDestroyOperator<int,int>
    {
        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            int removed =
                destructionSize +
                random.NextInt32(0, 5);

            partialSolution -=
                removed;

            return removed;
        }
    }

    private sealed class SyntheticRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        public void Repair(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution +=
                removedComponents -
                1 -
                random.NextInt32(0, 2);
        }
    }
}
