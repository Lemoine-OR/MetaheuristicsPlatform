using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class AdaptiveLargeNeighborhoodSearchBenchmarks
{
    private readonly AdaptiveLargeNeighborhoodSearchOptimizer<int,int> _optimizer =
        new(
            new ConstantInitial(),
            new[]
            {
                new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                    "destroy-a",
                    new SyntheticDestroy(0)),
                new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                    "destroy-b",
                    new SyntheticDestroy(2))
            },
            new[]
            {
                new AdaptiveLargeNeighborhoodRepairOperator<int,int>(
                    "repair-a",
                    new SyntheticRepair(0)),
                new AdaptiveLargeNeighborhoodRepairOperator<int,int>(
                    "repair-b",
                    new SyntheticRepair(1))
            },
            EqualityComparer<int>.Default);

    private readonly AdaptiveLargeNeighborhoodSearchParameters _parameters =
        new()
        {
            DestructionSize = 16,
            MaximumIterations = 500,
            SegmentLength = 50,
            InitialTemperature = 25.0,
            CoolingRate = 0.995
        };

    [Benchmark]
    public double AdaptiveLargeNeighborhoodSearch() =>
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

        public double Evaluate(int solution) =>
            Math.Abs(solution);
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
        private readonly int _bias;

        public SyntheticDestroy(int bias)
        {
            _bias = bias;
        }

        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            int removed =
                destructionSize +
                _bias +
                random.NextInt32(0, 5);

            partialSolution -= removed;
            return removed;
        }
    }

    private sealed class SyntheticRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        private readonly int _bias;

        public SyntheticRepair(int bias)
        {
            _bias = bias;
        }

        public void Repair(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution +=
                removedComponents -
                1 -
                _bias -
                random.NextInt32(0, 2);
        }
    }
}
