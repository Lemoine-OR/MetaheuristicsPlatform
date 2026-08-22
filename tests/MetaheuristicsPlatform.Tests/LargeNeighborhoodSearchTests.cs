using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class LargeNeighborhoodSearchTests
{
    [Fact]
    public void ImprovingOnlyAcceptanceIsStrictAndSenseSymmetric()
    {
        var random =
            new CountingRandomSource(0.5);

        var minimizeImproving =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                9.0,
                9.0);

        var minimizeEqual =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                10.0,
                10.0);

        var maximizeImproving =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Maximize,
                1,
                10.0,
                11.0,
                11.0);

        Assert.True(
            ImprovingOnlyLargeNeighborhoodAcceptancePolicy.Instance.ShouldAccept(
                in minimizeImproving,
                random));

        Assert.False(
            ImprovingOnlyLargeNeighborhoodAcceptancePolicy.Instance.ShouldAccept(
                in minimizeEqual,
                random));

        Assert.True(
            ImprovingOnlyLargeNeighborhoodAcceptancePolicy.Instance.ShouldAccept(
                in maximizeImproving,
                random));

        Assert.Equal(
            0,
            random.NextDoubleCalls);
    }

    [Fact]
    public void DestroyPrecedesRepairAndImprovingCandidateIsAccepted()
    {
        var trace =
            new List<string>();

        var optimizer =
            new LargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestroy(trace),
                new ImprovingRepair(trace));

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new LargeNeighborhoodSearchParameters
                {
                    DestructionSize = 2,
                    MaximumIterations = 1
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            new[] { "destroy:2", "repair:2" },
            trace);

        Assert.Equal(
            9,
            result.BestSolution);

        Assert.Equal(
            2,
            result.Statistics.Evaluations);

        Assert.Equal(
            1,
            result.Statistics.Iterations);

        Assert.Equal(
            "MaximumLargeNeighborhoodSearchIterations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void PartialSolutionIsNeverEvaluated()
    {
        var optimizer =
            new LargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(10),
                new SentinelDestroy(),
                new SentinelRepair());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new RejectPartialProblem(),
                new LargeNeighborhoodSearchParameters
                {
                    MaximumIterations = 1
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            9,
            result.BestSolution);
    }

    [Fact]
    public void EvaluationBudgetStopsBeforeIncompleteCycleIsCounted()
    {
        var optimizer =
            new LargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestroy(new List<string>()),
                new ImprovingRepair(new List<string>()));

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new LargeNeighborhoodSearchParameters
                {
                    DestructionSize = 2,
                    MaximumIterations = 10
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(2),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            2,
            result.Statistics.Evaluations);

        Assert.Equal(
            0,
            result.Statistics.Iterations);

        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);

        Assert.Equal(
            9,
            result.BestSolution);
    }

    [Fact]
    public void WorseningCandidateIsRejectedWithoutLosingBestSoFar()
    {
        var optimizer =
            new LargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestroy(new List<string>()),
                new WorseningRepair());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new LargeNeighborhoodSearchParameters
                {
                    DestructionSize = 2,
                    MaximumIterations = 1
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            10,
            result.BestSolution);

        Assert.Equal(
            10.0,
            result.BestFitness);
    }

    [Fact]
    public void SameSeedProducesSameDestroyRepairTrajectory()
    {
        OptimizationResult<int> first =
            RunSeeded();

        OptimizationResult<int> second =
            RunSeeded();

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);

        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
    }

    [Fact]
    public void StableIdCatalogAndTypedFactoryRegistrationAreAvailable()
    {
        Assert.Equal(
            "large-neighborhood-search-shaw-1998",
            MetaheuristicAlgorithmIds.LargeNeighborhoodSearch);

        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.LargeNeighborhoodSearch);

        Assert.True(
            entry.RequiresComposition);

        Assert.Equal(
            "10.1007/3-540-49481-2_30",
            entry.Doi);

        var configured =
            new LargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestroy(new List<string>()),
                new ImprovingRepair(new List<string>()));

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.LargeNeighborhoodSearch,
            () => configured,
            replace: true);

        LargeNeighborhoodSearchOptimizer<int,int> created =
            MetaheuristicFactory.Create<LargeNeighborhoodSearchOptimizer<int,int>>(
                MetaheuristicAlgorithmIds.LargeNeighborhoodSearch);

        Assert.Same(
            configured,
            created);
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new LargeNeighborhoodSearchParameters
                {
                    DestructionSize = 0
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new LargeNeighborhoodSearchParameters
                {
                    MaximumIterations = 0
                }.Validate());
    }

    private static OptimizationResult<int> RunSeeded()
    {
        var optimizer =
            new LargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(20),
                new RandomDestroy(),
                new RandomRepair());

        return optimizer.Optimize(
            new MinProblem(),
            new LargeNeighborhoodSearchParameters
            {
                DestructionSize = 3,
                MaximumIterations = 8
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            new OptimizationOptions { Seed = 20260822UL },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }

    private sealed class CountingRandomSource : IRandomSource
    {
        private readonly double _nextDouble;

        public CountingRandomSource(
            double nextDouble)
        {
            if (!double.IsFinite(nextDouble) ||
                nextDouble < 0.0 ||
                nextDouble >= 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(nextDouble));
            }

            _nextDouble =
                nextDouble;
        }

        public ulong Seed =>
            0UL;

        public int NextDoubleCalls { get; private set; }

        public ulong NextUInt64() =>
            0UL;

        public double NextDouble()
        {
            NextDoubleCalls++;

            return _nextDouble;
        }

        public int NextInt32(
            int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            return 0;
        }

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax)
        {
            if (inclusiveMin >= exclusiveMax)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            return inclusiveMin;
        }

        public void Fill(
            Span<byte> buffer)
        {
            buffer.Clear();
        }
    }

    private sealed class MinProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution) =>
            solution;
    }

    private sealed class RejectPartialProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution)
        {
            if (solution < 0)
            {
                throw new InvalidOperationException(
                    "A partial solution was evaluated.");
            }

            return solution;
        }
    }

    private sealed class ConstantInitial :
        INeighborhoodSearchInitialSolutionGenerator<int>
    {
        private readonly int _value;

        public ConstantInitial(
            int value)
        {
            _value =
                value;
        }

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            _value;
    }

    private sealed class RecordingDestroy :
        ILargeNeighborhoodDestroyOperator<int,int>
    {
        private readonly List<string> _trace;

        public RecordingDestroy(
            List<string> trace)
        {
            _trace =
                trace;
        }

        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add(
                $"destroy:{destructionSize}");

            partialSolution -=
                destructionSize;

            return destructionSize;
        }
    }

    private sealed class ImprovingRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        private readonly List<string> _trace;

        public ImprovingRepair(
            List<string> trace)
        {
            _trace =
                trace;
        }

        public void Repair(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add(
                $"repair:{removedComponents}");

            partialSolution +=
                removedComponents -
                1;
        }
    }

    private sealed class WorseningRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        public void Repair(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution +=
                removedComponents +
                2;
        }
    }

    private sealed class SentinelDestroy :
        ILargeNeighborhoodDestroyOperator<int,int>
    {
        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution =
                -999;

            return destructionSize;
        }
    }

    private sealed class SentinelRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        public void Repair(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution =
                9;
        }
    }

    private sealed class RandomDestroy :
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
                random.NextInt32(0, 3);

            partialSolution -=
                removed;

            return removed;
        }
    }

    private sealed class RandomRepair :
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
                random.NextInt32(0, 2);
        }
    }
}
