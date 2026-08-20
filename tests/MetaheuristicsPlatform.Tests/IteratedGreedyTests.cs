using MetaheuristicsPlatform.Algorithms.IteratedGreedy;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class IteratedGreedyTests
{
    [Fact]
    public void ImprovingOnlyAcceptanceIsStrictAndDeterministic()
    {
        var policy = ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance;
        var random = new FixedRandomSource(0.5);

        var improving = new IteratedGreedyAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 9.0, 9.0);
        var equal = new IteratedGreedyAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 10.0, 10.0);

        Assert.True(policy.ShouldAccept(in improving, random));
        Assert.False(policy.ShouldAccept(in equal, random));
        Assert.Equal(0, random.NextDoubleCalls);
    }

    [Fact]
    public void ConstantTemperatureAcceptanceUsesMetropolisProbability()
    {
        var policy = new ConstantTemperatureIteratedGreedyAcceptancePolicy(1.0);
        var candidate = new IteratedGreedyAcceptanceContext(
            OptimizationSense.Minimize, 1, 10.0, 12.0, 10.0);

        Assert.True(policy.ShouldAccept(in candidate, new FixedRandomSource(0.10)));
        Assert.False(policy.ShouldAccept(in candidate, new FixedRandomSource(0.20)));
    }

    [Fact]
    public void ConstantTemperatureAcceptanceMirrorsMaximization()
    {
        var policy = new ConstantTemperatureIteratedGreedyAcceptancePolicy(1.0);
        var candidate = new IteratedGreedyAcceptanceContext(
            OptimizationSense.Maximize, 1, 10.0, 8.0, 10.0);

        Assert.Equal(2.0, candidate.Degradation);
        Assert.True(policy.ShouldAccept(in candidate, new FixedRandomSource(0.10)));
        Assert.False(policy.ShouldAccept(in candidate, new FixedRandomSource(0.20)));
    }

    [Fact]
    public void DestructionPrecedesReconstructionAndImprovingCandidateIsAccepted()
    {
        var trace = new List<string>();
        var algorithm = new IteratedGreedyOptimizer<int,int>(
            new ConstantInitial(10),
            new RecordingDestruction(trace),
            new ImprovingConstruction(trace),
            ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance);

        var result = algorithm.Optimize(
            new MinProblem(),
            new IteratedGreedyParameters
            {
                DestructionSize = 2,
                MaximumIterations = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(9, result.BestSolution);
        Assert.Equal(new[] { "destroy:2", "construct:2" }, trace);
        Assert.Equal("MaximumIteratedGreedyIterations", result.StopDecision.Criterion);
    }

    [Fact]
    public void WorseningCandidateCanBeRejectedWithoutLosingBestSoFar()
    {
        var algorithm = new IteratedGreedyOptimizer<int,int>(
            new ConstantInitial(10),
            new RecordingDestruction(new List<string>()),
            new WorseningConstruction(),
            ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance);

        var result = algorithm.Optimize(
            new MinProblem(),
            new IteratedGreedyParameters
            {
                DestructionSize = 2,
                MaximumIterations = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(10.0, result.BestFitness);
    }

    [Fact]
    public void PartialSolutionIsNeverEvaluatedBeforeReconstruction()
    {
        var algorithm = new IteratedGreedyOptimizer<int,int>(
            new ConstantInitial(10),
            new SentinelDestruction(),
            new SentinelConstruction(),
            ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance);

        var result = algorithm.Optimize(
            new RejectPartialProblem(),
            new IteratedGreedyParameters { MaximumIterations = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(9, result.BestSolution);
    }

    [Fact]
    public void OptionalLocalSearchRunsOnInitialAndReconstructedSolutions()
    {
        var localSearch = new DecrementLocalSearch();
        var algorithm = new IteratedGreedyOptimizer<int,int>(
            new ConstantInitial(10),
            new RecordingDestruction(new List<string>()),
            new ImprovingConstruction(new List<string>()),
            ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance,
            localSearch);

        var result = algorithm.Optimize(
            new MinProblem(),
            new IteratedGreedyParameters
            {
                DestructionSize = 2,
                MaximumIterations = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(7, result.BestSolution);
        Assert.Equal(2, localSearch.Invocations);
    }

    [Fact]
    public void StableIdAndCatalogExposeIteratedGreedy()
    {
        Assert.Equal(
            "iterated-greedy-ruiz-stutzle-2007",
            MetaheuristicAlgorithmIds.IteratedGreedy);

        var entry = MetaheuristicCatalog.GetRequired(
            MetaheuristicAlgorithmIds.IteratedGreedy);

        Assert.True(entry.RequiresComposition);
        Assert.Equal("10.1016/j.ejor.2005.12.009", entry.Doi);
    }

    [Fact]
    public void InvalidParametersAndTemperatureAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IteratedGreedyParameters { DestructionSize = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IteratedGreedyParameters { MaximumIterations = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConstantTemperatureIteratedGreedyAcceptancePolicy(0.0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConstantTemperatureIteratedGreedyAcceptancePolicy(double.NaN));
    }

    private sealed class MinProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) => solution;
    }

    private sealed class RejectPartialProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution)
        {
            if (solution < 0)
                throw new InvalidOperationException("A partial solution was evaluated.");

            return solution;
        }
    }

    private sealed class ConstantInitial : INeighborhoodSearchInitialSolutionGenerator<int>
    {
        private readonly int _value;

        public ConstantInitial(int value) => _value = value;

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) => _value;
    }

    private sealed class RecordingDestruction : IIteratedGreedyDestruction<int,int>
    {
        private readonly List<string> _trace;

        public RecordingDestruction(List<string> trace) => _trace = trace;

        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add($"destroy:{destructionSize}");
            partialSolution -= destructionSize;
            return destructionSize;
        }
    }

    private sealed class ImprovingConstruction : IIteratedGreedyConstruction<int,int>
    {
        private readonly List<string> _trace;

        public ImprovingConstruction(List<string> trace) => _trace = trace;

        public void Reconstruct(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add($"construct:{removedComponents}");
            partialSolution += removedComponents - 1;
        }
    }

    private sealed class WorseningConstruction : IIteratedGreedyConstruction<int,int>
    {
        public void Reconstruct(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution += removedComponents + 2;
        }
    }

    private sealed class SentinelDestruction : IIteratedGreedyDestruction<int,int>
    {
        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution = -999;
            return 9;
        }
    }

    private sealed class SentinelConstruction : IIteratedGreedyConstruction<int,int>
    {
        public void Reconstruct(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution = removedComponents;
        }
    }

    private sealed class DecrementLocalSearch : ILocalSearchProcedure<int>
    {
        public int Invocations { get; private set; }

        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Invocations++;
            solution--;
            double fitness = context.Evaluate(solution);

            return new LocalSearchProcedureResult(
                fitness,
                1,
                false,
                StoppingDecision.Continue("TestLocalSearch"));
        }
    }

    private sealed class FixedRandomSource : IRandomSource
    {
        private readonly double _nextDouble;

        public FixedRandomSource(double nextDouble) => _nextDouble = nextDouble;

        public ulong Seed => 1UL;
        public int NextDoubleCalls { get; private set; }

        public ulong NextUInt64() => 0UL;

        public double NextDouble()
        {
            NextDoubleCalls++;
            return _nextDouble;
        }

        public int NextInt32(int exclusiveMax) => 0;

        public int NextInt32(int inclusiveMin, int exclusiveMax) => inclusiveMin;

        public void Fill(Span<byte> buffer) => buffer.Clear();
    }
}
