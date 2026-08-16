using MetaheuristicsPlatform.Algorithms.Constructive;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ReactiveGraspTests
{
    [Fact]
    public void InitialReactiveProbabilitiesAreUniform()
    {
        var controller =
            new PraisRibeiroReactiveAlphaController(
                new[] { 0.1, 0.5, 0.9 },
                probabilityUpdatePeriod: 10,
                OptimizationSense.Minimize);

        Assert.Equal(1.0 / 3.0, controller.GetProbability(0), 12);
        Assert.Equal(1.0 / 3.0, controller.GetProbability(1), 12);
        Assert.Equal(1.0 / 3.0, controller.GetProbability(2), 12);
        Assert.Equal(0, controller.ProbabilityUpdates);
    }

    [Fact]
    public void MinimizationRatioUpdateFavorsLowerAverageObjective()
    {
        var controller =
            new PraisRibeiroReactiveAlphaController(
                new[] { 0.1, 0.9 },
                probabilityUpdatePeriod: 2,
                OptimizationSense.Minimize);

        controller.Observe(0, 10.0);
        controller.Observe(1, 20.0);

        Assert.Equal(2.0 / 3.0, controller.GetProbability(0), 12);
        Assert.Equal(1.0 / 3.0, controller.GetProbability(1), 12);
        Assert.Equal(1, controller.ProbabilityUpdates);
    }

    [Fact]
    public void MaximizationMirrorRatioFavorsHigherAverageObjective()
    {
        var controller =
            new PraisRibeiroReactiveAlphaController(
                new[] { 0.1, 0.9 },
                probabilityUpdatePeriod: 2,
                OptimizationSense.Maximize);

        controller.Observe(0, 20.0);
        controller.Observe(1, 10.0);

        Assert.Equal(2.0 / 3.0, controller.GetProbability(0), 12);
        Assert.Equal(1.0 / 3.0, controller.GetProbability(1), 12);
        Assert.Equal(1, controller.ProbabilityUpdates);
    }

    [Fact]
    public void ProbabilityUpdateWaitsUntilEveryAlphaHasBeenObserved()
    {
        var controller =
            new PraisRibeiroReactiveAlphaController(
                new[] { 0.1, 0.9 },
                probabilityUpdatePeriod: 1,
                OptimizationSense.Minimize);

        controller.Observe(0, 10.0);

        Assert.Equal(0, controller.ProbabilityUpdates);
        Assert.Equal(1, controller.DistinctObserved);

        controller.Observe(1, 20.0);

        Assert.Equal(1, controller.ProbabilityUpdates);
        Assert.Equal(2, controller.DistinctObserved);
    }

    [Fact]
    public void CanonicalRatioUpdateRejectsZeroOrNegativeObjectives()
    {
        var controller =
            new PraisRibeiroReactiveAlphaController(
                new[] { 0.1, 0.9 },
                probabilityUpdatePeriod: 2,
                OptimizationSense.Minimize);

        controller.Observe(0, 10.0);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => controller.Observe(1, 0.0));

        Assert.True(
            exception.Message.Contains(
                "strictly positive",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RouletteSelectionUsesCurrentReactiveProbabilities()
    {
        var controller =
            new PraisRibeiroReactiveAlphaController(
                new[] { 0.1, 0.9 },
                probabilityUpdatePeriod: 2,
                OptimizationSense.Minimize);

        controller.Observe(0, 10.0);
        controller.Observe(1, 20.0);

        int selected =
            controller.SelectAlphaIndex(
                new FixedDoubleRandomSource(0.80));

        Assert.Equal(1, selected);
        Assert.Equal(0.9, controller.GetAlpha(selected), 12);
    }

    [Fact]
    public void ReactiveParametersRejectInvalidAlphaSets()
    {
        Assert.Throws<ArgumentException>(() =>
            new ReactiveGraspParameters
            {
                AlphaValues = Array.Empty<double>()
            }.Validate());

        Assert.Throws<ArgumentException>(() =>
            new ReactiveGraspParameters
            {
                AlphaValues = new[] { 0.2, 0.2 }
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ReactiveGraspParameters
            {
                AlphaValues = new[] { -0.1, 0.5 }
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ReactiveGraspParameters
            {
                ProbabilityUpdatePeriod = 0
            }.Validate());
    }

    [Fact]
    public void ReactiveOptimizerUsesCommonIterationStoppingLifecycle()
    {
        var optimizer =
            new ReactiveGraspOptimizer<int>(
                new PositiveFixedConstructionProcedure(),
                new NoOpLocalSearchProcedure());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new PositiveMinimizationProblem(),
                new ReactiveGraspParameters
                {
                    MaximumIterations = 20,
                    AlphaValues = new[] { 0.2 },
                    ProbabilityUpdatePeriod = 1
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(3),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("MaxIterations", result.StopDecision.Criterion);
        Assert.Equal("reactive-grasp-prais-ribeiro-2000", result.Algorithm.Id);
    }

    [Fact]
    public void CanonicalGraspNowUsesCommonIterationStoppingLifecycle()
    {
        var optimizer =
            new GraspOptimizer<int>(
                new PositiveFixedConstructionProcedure(),
                new NoOpLocalSearchProcedure());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new PositiveMinimizationProblem(),
                new GraspParameters
                {
                    MaximumIterations = 20,
                    Alpha = 0.2
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(2),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("MaxIterations", result.StopDecision.Criterion);
    }

    [Fact]
    public void StableIdAndRuntimeCatalogExposeReactiveGrasp()
    {
        Assert.Equal(
            "reactive-grasp-prais-ribeiro-2000",
            MetaheuristicAlgorithmIds.ReactiveGrasp);

        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.ReactiveGrasp);

        Assert.Equal("constructive-methods", entry.Category);
        Assert.True(entry.RequiresComposition);
    }

    [Fact]
    public void ReactiveDescriptorCarriesPraisRibeiroReference()
    {
        var optimizer =
            new ReactiveGraspOptimizer<int>(
                new PositiveFixedConstructionProcedure(),
                new NoOpLocalSearchProcedure());

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1287/ijoc.12.3.164.12639");
    }

    private sealed class PositiveMinimizationProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Math.Max(1.0, solution);
    }

    private sealed class PositiveFixedConstructionProcedure :
        IGraspConstructionProcedure<int>
    {
        public GraspConstructionResult<int> Construct(
            IOptimizationProblem<int> problem,
            IRandomSource random,
            double alpha,
            int maximumConstructionSteps,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new GraspConstructionResult<int>(
                Solution: 10,
                ConstructionSteps: 1,
                GreedyScoreEvaluations: 1);
        }
    }

    private sealed class NoOpLocalSearchProcedure :
        ILocalSearchProcedure<int>
    {
        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new LocalSearchProcedureResult(
                currentFitness,
                acceptedMoves: 0,
                localOptimum: true,
                StoppingDecision.Continue("NoOpLocalSearch"));
        }
    }

    private sealed class FixedDoubleRandomSource :
        IRandomSource
    {
        private readonly double _value;

        public FixedDoubleRandomSource(double value)
        {
            _value = value;
        }

        public ulong Seed => 1UL;

        public ulong NextUInt64() => 0UL;

        public double NextDouble() => _value;

        public int NextInt32(int exclusiveMax) => 0;

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax) =>
            inclusiveMin;

        public void Fill(Span<byte> buffer) =>
            buffer.Clear();
    }
}
