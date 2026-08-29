using MetaheuristicsPlatform.Algorithms.HyperHeuristics.FuzzyAdaptiveLateAcceptanceHyperHeuristic;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class FuzzyAdaptiveLateAcceptanceHyperHeuristicOptimizerTests
{
    [Fact]
    public void Optimize_UsesDomainHeuristicPool_AndFactoryCreatesCanonicalType()
    {
        TestDomain domain = new();

        HyperHeuristicOptimizationResult result =
            new FuzzyAdaptiveLateAcceptanceHyperHeuristicOptimizer().Optimize(
                domain,
                new FuzzyAdaptiveLateAcceptanceHyperHeuristicParameters
                {
                    MaximumIterations = 24
                },
                new OptimizationOptions
                {
                    Seed = 44556677UL
                },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Evaluations > 0);
        Assert.True(double.IsFinite(result.BestObjective));
        Assert.NotEmpty(result.HeuristicTrace);
        Assert.True(result.BestObjective <= 16.0);

        Assert.IsType<FuzzyAdaptiveLateAcceptanceHyperHeuristicOptimizer>(
            MetaheuristicFactory.Create<FuzzyAdaptiveLateAcceptanceHyperHeuristicOptimizer>(
                MetaheuristicAlgorithmIds.FuzzyAdaptiveLateAcceptanceHyperHeuristic));
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
            VectorSolution vector =
                Assert.IsType<VectorSolution>(solution);

            vector.Value *= 0.5;
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
            VectorSolution vector =
                Assert.IsType<VectorSolution>(solution);

            vector.Value -=
                Math.Sign(vector.Value) *
                Math.Min(Math.Abs(vector.Value), 0.25);
        }
    }

    private sealed class TestDomain :
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
            new VectorSolution(4.0);

        public double Evaluate(
            IHyperHeuristicSolution solution)
        {
            VectorSolution vector =
                Assert.IsType<VectorSolution>(solution);

            return vector.Value * vector.Value;
        }

        public double[] Describe(
            IHyperHeuristicSolution solution)
        {
            VectorSolution vector =
                Assert.IsType<VectorSolution>(solution);

            return new[] { vector.Value, Math.Abs(vector.Value) };
        }
    }
}
