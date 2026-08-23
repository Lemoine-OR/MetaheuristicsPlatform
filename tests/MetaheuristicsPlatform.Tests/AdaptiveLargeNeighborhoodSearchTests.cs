using MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdaptiveLargeNeighborhoodSearchTests
{
    [Fact]
    public void RouletteSelectionUsesPublishedWeightProportions()
    {
        var random =
            new CountingRandomSource(
                nextDouble: 0.30);

        int index =
            AdaptiveLargeNeighborhoodAdaptation.SelectIndex(
                new[] { 1.0, 3.0 },
                random);

        Assert.Equal(1, index);
        Assert.Equal(1, random.NextDoubleCalls);
    }

    [Fact]
    public void SegmentWeightUpdateMatchesRopkePisingerFormula()
    {
        double updated =
            AdaptiveLargeNeighborhoodAdaptation.UpdateWeight(
                currentWeight: 2.0,
                accumulatedScore: 12.0,
                usageCount: 3,
                reactionFactor: 0.25);

        Assert.Equal(2.5, updated, 12);

        Assert.Equal(
            2.0,
            AdaptiveLargeNeighborhoodAdaptation.UpdateWeight(
                currentWeight: 2.0,
                accumulatedScore: 0.0,
                usageCount: 0,
                reactionFactor: 1.0),
            12);
    }

    [Fact]
    public void NovelOutcomeRewardTiersAreCanonical()
    {
        var parameters =
            new AdaptiveLargeNeighborhoodSearchParameters();

        Assert.Equal(
            33.0,
            AdaptiveLargeNeighborhoodAdaptation.DetermineReward(
                true, true, true, true, parameters));

        Assert.Equal(
            9.0,
            AdaptiveLargeNeighborhoodAdaptation.DetermineReward(
                true, false, true, true, parameters));

        Assert.Equal(
            13.0,
            AdaptiveLargeNeighborhoodAdaptation.DetermineReward(
                true, false, false, true, parameters));

        Assert.Equal(
            0.0,
            AdaptiveLargeNeighborhoodAdaptation.DetermineReward(
                false, true, true, true, parameters));
    }

    [Fact]
    public void GeometricMetropolisAcceptanceIsSenseSymmetric()
    {
        var acceptance =
            new GeometricSimulatedAnnealingLargeNeighborhoodAcceptancePolicy(
                initialTemperature: 1.0,
                coolingRate: 1.0);

        var random =
            new CountingRandomSource(
                nextDouble: 0.999999);

        var improvingMin =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Minimize,
                1,
                10.0,
                9.0,
                9.0);

        var worseningMax =
            new LargeNeighborhoodAcceptanceContext(
                OptimizationSense.Maximize,
                1,
                10.0,
                0.0,
                10.0);

        Assert.True(
            acceptance.ShouldAccept(
                in improvingMin,
                random));

        Assert.False(
            acceptance.ShouldAccept(
                in worseningMax,
                random));

        Assert.Equal(1, random.NextDoubleCalls);
    }

    [Fact]
    public void OneIterationUsesExactlyOneDestroyAndOneRepair()
    {
        var destroy =
            new CountingDestroy();

        var repair =
            new ImprovingRepair();

        var optimizer =
            CreateOptimizer(
                destroy,
                repair);

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new AdaptiveLargeNeighborhoodSearchParameters
                {
                    DestructionSize = 2,
                    MaximumIterations = 1,
                    SegmentLength = 1,
                    InitialTemperature = 1.0,
                    CoolingRate = 1.0
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                new OptimizationOptions { Seed = 123UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(1, destroy.Calls);
        Assert.Equal(1, repair.Calls);
        Assert.Equal(2, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
        Assert.Equal(9, result.BestSolution);
    }

    [Fact]
    public void EvaluationBudgetStopsBeforeIncompleteAdaptiveCycleIsCounted()
    {
        var optimizer =
            CreateOptimizer(
                new CountingDestroy(),
                new ImprovingRepair());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new MinProblem(),
                new AdaptiveLargeNeighborhoodSearchParameters
                {
                    MaximumIterations = 10
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(2),
                new OptimizationOptions { Seed = 456UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
        Assert.Equal("MaxEvaluations", result.StopDecision.Criterion);
        Assert.Equal(9, result.BestSolution);
    }

    [Fact]
    public void SameSeedProducesSameAdaptiveTrajectory()
    {
        OptimizationResult<int> first =
            RunSeeded();

        OptimizationResult<int> second =
            RunSeeded();

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(first.Statistics.Evaluations, second.Statistics.Evaluations);
    }

    [Fact]
    public void DuplicateOperatorIdsAreRejected()
    {
        var destroy =
            new CountingDestroy();

        var repair =
            new ImprovingRepair();

        Assert.Throws<ArgumentException>(
            () =>
                new AdaptiveLargeNeighborhoodSearchOptimizer<int,int>(
                    new ConstantInitial(10),
                    new[]
                    {
                        new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                            "same",
                            destroy),
                        new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                            "same",
                            destroy)
                    },
                    new[]
                    {
                        new AdaptiveLargeNeighborhoodRepairOperator<int,int>(
                            "repair",
                            repair)
                    },
                    EqualityComparer<int>.Default));
    }

    [Fact]
    public void StableIdCatalogAndTypedFactoryRegistrationAreAvailable()
    {
        Assert.Equal(
            "adaptive-large-neighborhood-search-ropke-pisinger-2006",
            MetaheuristicAlgorithmIds.AdaptiveLargeNeighborhoodSearch);

        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.AdaptiveLargeNeighborhoodSearch);

        Assert.True(entry.RequiresComposition);
        Assert.Equal("10.1287/trsc.1050.0135", entry.Doi);

        var configured =
            CreateOptimizer(
                new CountingDestroy(),
                new ImprovingRepair());

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.AdaptiveLargeNeighborhoodSearch,
            () => configured,
            replace: true);

        AdaptiveLargeNeighborhoodSearchOptimizer<int,int> created =
            MetaheuristicFactory.Create<AdaptiveLargeNeighborhoodSearchOptimizer<int,int>>(
                MetaheuristicAlgorithmIds.AdaptiveLargeNeighborhoodSearch);

        Assert.Same(configured, created);
    }

    private static AdaptiveLargeNeighborhoodSearchOptimizer<int,int>
        CreateOptimizer(
            CountingDestroy destroy,
            ImprovingRepair repair) =>
        new(
            new ConstantInitial(10),
            new[]
            {
                new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                    "destroy",
                    destroy)
            },
            new[]
            {
                new AdaptiveLargeNeighborhoodRepairOperator<int,int>(
                    "repair",
                    repair)
            },
            EqualityComparer<int>.Default);

    private static OptimizationResult<int> RunSeeded()
    {
        var optimizer =
            new AdaptiveLargeNeighborhoodSearchOptimizer<int,int>(
                new ConstantInitial(20),
                new[]
                {
                    new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                        "d1",
                        new RandomDestroy(0)),
                    new AdaptiveLargeNeighborhoodDestroyOperator<int,int>(
                        "d2",
                        new RandomDestroy(1))
                },
                new[]
                {
                    new AdaptiveLargeNeighborhoodRepairOperator<int,int>(
                        "r1",
                        new RandomRepair(0)),
                    new AdaptiveLargeNeighborhoodRepairOperator<int,int>(
                        "r2",
                        new RandomRepair(1))
                },
                EqualityComparer<int>.Default);

        return optimizer.Optimize(
            new MinProblem(),
            new AdaptiveLargeNeighborhoodSearchParameters
            {
                DestructionSize = 2,
                MaximumIterations = 12,
                SegmentLength = 4,
                InitialTemperature = 2.0,
                CoolingRate = 0.99
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            new OptimizationOptions { Seed = 20260822UL },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }

    private sealed class MinProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            solution;
    }

    private sealed class ConstantInitial :
        INeighborhoodSearchInitialSolutionGenerator<int>
    {
        private readonly int _value;

        public ConstantInitial(int value)
        {
            _value = value;
        }

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            _value;
    }

    private sealed class CountingDestroy :
        ILargeNeighborhoodDestroyOperator<int,int>
    {
        public int Calls { get; private set; }

        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            Calls++;
            partialSolution -= destructionSize;
            return destructionSize;
        }
    }

    private sealed class ImprovingRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        public int Calls { get; private set; }

        public void Repair(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            Calls++;
            partialSolution += removedComponents - 1;
        }
    }

    private sealed class RandomDestroy :
        ILargeNeighborhoodDestroyOperator<int,int>
    {
        private readonly int _bias;

        public RandomDestroy(int bias)
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
                random.NextInt32(0, 2);

            partialSolution -= removed;
            return removed;
        }
    }

    private sealed class RandomRepair :
        ILargeNeighborhoodRepairOperator<int,int>
    {
        private readonly int _bias;

        public RandomRepair(int bias)
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
                _bias -
                random.NextInt32(0, 2);
        }
    }

    private sealed class CountingRandomSource : IRandomSource
    {
        private readonly double _nextDouble;

        public CountingRandomSource(double nextDouble)
        {
            if (!double.IsFinite(nextDouble) ||
                nextDouble < 0.0 ||
                nextDouble >= 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(nextDouble));
            }

            _nextDouble = nextDouble;
        }

        public ulong Seed => 0UL;
        public int NextDoubleCalls { get; private set; }
        public ulong NextUInt64() => 0UL;

        public double NextDouble()
        {
            NextDoubleCalls++;
            return _nextDouble;
        }

        public int NextInt32(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            return 0;
        }

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax)
        {
            if (inclusiveMin >= exclusiveMax)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            return inclusiveMin;
        }

        public void Fill(Span<byte> buffer)
        {
            buffer.Clear();
        }
    }
}
