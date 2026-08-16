using MetaheuristicsPlatform.Algorithms.Constructive;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class GraspTests
{
    [Fact]
    public void AlphaZeroKeepsOnlyGreedyBestCandidateForMinimizationScore()
    {
        var procedure =
            new CanonicalGraspConstructionProcedure<int, int, ThreeCandidateEnumerator>(
                new OneStepConstructionModel(GraspGreedyScoreSense.Minimize));

        GraspConstructionResult<int> result =
            procedure.Construct(
                new DistanceProblem(),
                new AlwaysReplaceRandomSource(),
                alpha: 0.0,
                maximumConstructionSteps: 10,
                TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Solution);
        Assert.Equal(1, result.ConstructionSteps);
        Assert.Equal(6, result.GreedyScoreEvaluations);
    }

    [Fact]
    public void AlphaOneAdmitsEntireCandidateListAndUsesUniformReservoirSelection()
    {
        var procedure =
            new CanonicalGraspConstructionProcedure<int, int, ThreeCandidateEnumerator>(
                new OneStepConstructionModel(GraspGreedyScoreSense.Minimize));

        GraspConstructionResult<int> result =
            procedure.Construct(
                new DistanceProblem(),
                new AlwaysReplaceRandomSource(),
                alpha: 1.0,
                maximumConstructionSteps: 10,
                TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Solution);
    }

    [Fact]
    public void AlphaZeroRespectsMaximizationGreedyScore()
    {
        var procedure =
            new CanonicalGraspConstructionProcedure<int, int, ThreeCandidateEnumerator>(
                new OneStepConstructionModel(GraspGreedyScoreSense.Maximize));

        GraspConstructionResult<int> result =
            procedure.Construct(
                new DistanceProblem(),
                new AlwaysReplaceRandomSource(),
                alpha: 0.0,
                maximumConstructionSteps: 10,
                TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Solution);
    }

    [Fact]
    public void ConstructionIsAdaptiveAndRecomputesCandidateScoresAfterEachSelection()
    {
        var model = new AdaptiveTargetConstructionModel();

        var procedure =
            new CanonicalGraspConstructionProcedure<int, int, ThreeCandidateEnumerator>(
                model);

        GraspConstructionResult<int> result =
            procedure.Construct(
                new DistanceProblem(),
                new AlwaysReplaceRandomSource(),
                alpha: 0.0,
                maximumConstructionSteps: 10,
                TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Solution);
        Assert.Equal(2, result.ConstructionSteps);
        Assert.Equal(12, result.GreedyScoreEvaluations);
        Assert.Equal(12, model.ScoreCalls);
    }

    [Fact]
    public void ConstructionRejectsNonFiniteGreedyScores()
    {
        var procedure =
            new CanonicalGraspConstructionProcedure<int, int, ThreeCandidateEnumerator>(
                new NonFiniteConstructionModel());

        Assert.Throws<InvalidOperationException>(() =>
            procedure.Construct(
                new DistanceProblem(),
                new AlwaysReplaceRandomSource(),
                alpha: 0.2,
                maximumConstructionSteps: 10,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ConstructionRejectsIncompleteSolutionWithNoCandidates()
    {
        var procedure =
            new CanonicalGraspConstructionProcedure<int, int, EmptyCandidateEnumerator>(
                new EmptyConstructionModel());

        Assert.Throws<InvalidOperationException>(() =>
            procedure.Construct(
                new DistanceProblem(),
                new AlwaysReplaceRandomSource(),
                alpha: 0.2,
                maximumConstructionSteps: 10,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void GraspOptimizerComposesConstructionAndReusableLocalSearch()
    {
        var optimizer =
            new GraspOptimizer<int>(
                new FixedConstructionProcedure(5),
                new TargetLocalSearchProcedure());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new DistanceProblem(),
                new GraspParameters
                {
                    MaximumIterations = 3,
                    Alpha = 0.2
                },
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness);
        Assert.Equal("MaximumGraspIterations", result.StopDecision.Criterion);
    }

    [Fact]
    public void GraspParametersRejectInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspParameters { MaximumIterations = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspParameters { Alpha = -0.01 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspParameters { Alpha = 1.01 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspParameters { MaximumConstructionSteps = 0 }.Validate());
    }

    [Fact]
    public void StableIdAndRuntimeCatalogExposeCanonicalGrasp()
    {
        Assert.Equal(
            "grasp-feo-resende-1995",
            MetaheuristicAlgorithmIds.Grasp);

        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.Grasp);

        Assert.Equal("constructive-methods", entry.Category);
        Assert.True(entry.RequiresComposition);
    }

    [Fact]
    public void DescriptorCarriesCanonicalFeoResendeReference()
    {
        var optimizer =
            new GraspOptimizer<int>(
                new FixedConstructionProcedure(5),
                new TargetLocalSearchProcedure());

        Assert.Equal(
            "grasp-feo-resende-1995",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1007/BF01096763");
    }

    private sealed class DistanceProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Math.Abs(10 - solution);
    }

    private struct ThreeCandidateEnumerator :
        IGraspCandidateEnumerator<int>
    {
        private int _index;

        public ThreeCandidateEnumerator()
        {
            _index = 0;
        }

        public bool MoveNext(out int candidate)
        {
            _index++;

            if (_index <= 3)
            {
                candidate = _index;
                return true;
            }

            candidate = default;
            return false;
        }
    }

    private readonly struct EmptyCandidateEnumerator :
        IGraspCandidateEnumerator<int>
    {
        public bool MoveNext(out int candidate)
        {
            candidate = default;
            return false;
        }
    }

    private sealed class OneStepConstructionModel :
        IGraspConstructionModel<int, int, ThreeCandidateEnumerator>
    {
        public OneStepConstructionModel(GraspGreedyScoreSense sense)
        {
            GreedyScoreSense = sense;
        }

        public GraspGreedyScoreSense GreedyScoreSense { get; }

        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            solution != 0;

        public ThreeCandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public double EvaluateGreedyScore(
            in int solution,
            in int candidate,
            IOptimizationProblem<int> problem) =>
            candidate;

        public void ApplyCandidate(
            ref int solution,
            in int candidate,
            IOptimizationProblem<int> problem) =>
            solution = candidate;
    }

    private sealed class AdaptiveTargetConstructionModel :
        IGraspConstructionModel<int, int, ThreeCandidateEnumerator>
    {
        public int ScoreCalls { get; private set; }

        public GraspGreedyScoreSense GreedyScoreSense =>
            GraspGreedyScoreSense.Minimize;

        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            solution >= 5;

        public ThreeCandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public double EvaluateGreedyScore(
            in int solution,
            in int candidate,
            IOptimizationProblem<int> problem)
        {
            ScoreCalls++;
            return Math.Abs(5 - (solution + candidate));
        }

        public void ApplyCandidate(
            ref int solution,
            in int candidate,
            IOptimizationProblem<int> problem) =>
            solution += candidate;
    }

    private sealed class NonFiniteConstructionModel :
        IGraspConstructionModel<int, int, ThreeCandidateEnumerator>
    {
        public GraspGreedyScoreSense GreedyScoreSense =>
            GraspGreedyScoreSense.Minimize;

        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            false;

        public ThreeCandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public double EvaluateGreedyScore(
            in int solution,
            in int candidate,
            IOptimizationProblem<int> problem) =>
            double.NaN;

        public void ApplyCandidate(
            ref int solution,
            in int candidate,
            IOptimizationProblem<int> problem)
        {
        }
    }

    private sealed class EmptyConstructionModel :
        IGraspConstructionModel<int, int, EmptyCandidateEnumerator>
    {
        public GraspGreedyScoreSense GreedyScoreSense =>
            GraspGreedyScoreSense.Minimize;

        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            false;

        public EmptyCandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public double EvaluateGreedyScore(
            in int solution,
            in int candidate,
            IOptimizationProblem<int> problem) =>
            0.0;

        public void ApplyCandidate(
            ref int solution,
            in int candidate,
            IOptimizationProblem<int> problem)
        {
        }
    }

    private sealed class FixedConstructionProcedure :
        IGraspConstructionProcedure<int>
    {
        private readonly int _solution;

        public FixedConstructionProcedure(int solution)
        {
            _solution = solution;
        }

        public GraspConstructionResult<int> Construct(
            IOptimizationProblem<int> problem,
            IRandomSource random,
            double alpha,
            int maximumConstructionSteps,
            CancellationToken cancellationToken) =>
            new(_solution, 1, 1);
    }

    private sealed class TargetLocalSearchProcedure :
        ILocalSearchProcedure<int>
    {
        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            solution = 10;
            double fitness = context.Evaluate(solution);

            return new LocalSearchProcedureResult(
                fitness,
                acceptedMoves: 1,
                localOptimum: true,
                StoppingDecision.Continue("TargetLocalSearch"));
        }
    }

    private sealed class NeverStoppingCriterion : IStoppingCriterion
    {
        public string Name => "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }

    private sealed class AlwaysReplaceRandomSource : IRandomSource
    {
        public ulong Seed => 1UL;

        public ulong NextUInt64() => 0UL;

        public double NextDouble() => 0.0;

        public int NextInt32(int exclusiveMax) => 0;

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax) =>
            inclusiveMin;

        public void Fill(Span<byte> buffer) =>
            buffer.Clear();
    }
}
