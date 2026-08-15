using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class LocalSearchFoundationTests
{
    [Fact]
    public void BestImprovementLocalSearchReachesLineOptimum()
    {
        var optimizer = new BestImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), new StepMoveOperator(), new QuadraticDelta(3));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(3),
            new LocalSearchParameters(),
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void FirstImprovementLocalSearchReachesLineOptimum()
    {
        var optimizer = new FirstImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), new StepMoveOperator(), new QuadraticDelta(3));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(3),
            new LocalSearchParameters(),
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, result.BestSolution);
    }

    [Fact]
    public void BestImprovementChoosesSteepestCandidateOnFirstStep()
    {
        var optimizer = new BestImprovementLocalSearchOptimizer<int, StepMove, int, TwoStepEnumerator>(
            Initial(0), new TwoStepNeighborhood(), new StepMoveOperator(), new QuadraticDelta(2));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(2),
            new LocalSearchParameters { MaximumAcceptedMoves = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.BestSolution);
    }

    [Fact]
    public void FirstImprovementRespectsEnumerationOrder()
    {
        var optimizer = new FirstImprovementLocalSearchOptimizer<int, StepMove, int, TwoStepEnumerator>(
            Initial(0), new TwoStepNeighborhood(), new StepMoveOperator(), new QuadraticDelta(2));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(2),
            new LocalSearchParameters { MaximumAcceptedMoves = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result.BestSolution);
    }

    [Fact]
    public void DeltaPathAppliesOnlySelectedMove()
    {
        var moveOperator = new StepMoveOperator();
        var optimizer = new BestImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), moveOperator, new QuadraticDelta(1));

        _ = optimizer.Optimize(
            new QuadraticProblem(1),
            new LocalSearchParameters { MaximumAcceptedMoves = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, moveOperator.ApplyCount);
        Assert.Equal(0, moveOperator.UndoCount);
    }

    [Fact]
    public void FullEvaluationFallbackRestoresCandidates()
    {
        var moveOperator = new StepMoveOperator();
        var optimizer = new BestImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), moveOperator);

        _ = optimizer.Optimize(
            new QuadraticProblem(1),
            new LocalSearchParameters { MaximumAcceptedMoves = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(moveOperator.ApplyCount >= 3);
        Assert.True(moveOperator.UndoCount >= 2);
    }

    [Fact]
    public void EvaluationBudgetStopsInsideScanWithoutOvershoot()
    {
        var moveOperator = new StepMoveOperator();
        var optimizer = new BestImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), moveOperator, new QuadraticDelta(2));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(2),
            new LocalSearchParameters(),
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("MaxEvaluations", result.StopDecision.Criterion);
        Assert.Equal(2, result.Statistics.Evaluations);
        Assert.Equal(0, moveOperator.ApplyCount);
        Assert.Equal(0, result.BestSolution);
    }

    [Fact]
    public void BestImprovementSupportsMaximization()
    {
        var optimizer = new BestImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), new StepMoveOperator(), new PeakDelta(2));

        OptimizationResult<int> result = optimizer.Optimize(
            new PeakProblem(2),
            new LocalSearchParameters(),
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void LocalSearchParametersRejectNonPositiveLimit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LocalSearchParameters { MaximumAcceptedMoves = 0 }.Validate());
    }

    [Fact]
    public void MoveProcedureRejectsUnknownSelectionPolicy()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MoveLocalSearchProcedure<int, StepMove, int, LineEnumerator>(
                new LineNeighborhood(),
                new StepMoveOperator(),
                (LocalSearchSelectionPolicy)999));
    }

    [Fact]
    public void CatalogContainsBothLocalSearchCoreIds()
    {
        string[] ids =
        [
            "local-search-best-improvement",
            "local-search-first-improvement"
        ];

        foreach (string id in ids)
        {
            MetaheuristicCatalogEntry entry = MetaheuristicCatalog.GetRequired(id);
            Assert.Equal(id, entry.Id);
            Assert.True(entry.RequiresComposition);
        }
    }

    [Fact]
    public void LocalSearchDescriptorsRemainDistinct()
    {
        var best = new BestImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), new StepMoveOperator(), new QuadraticDelta(1));
        var first = new FirstImprovementLocalSearchOptimizer<int, StepMove, int, LineEnumerator>(
            Initial(0), new LineNeighborhood(), new StepMoveOperator(), new QuadraticDelta(1));

        Assert.NotEqual(best.Descriptor.Id, first.Descriptor.Id);
        Assert.Equal("LS-BI", best.Descriptor.Acronym);
        Assert.Equal("LS-FI", first.Descriptor.Acronym);
    }

    [Fact]
    public void PublicAlgorithmIdsExposeLocalSearchCore()
    {
        Assert.Equal(
            "local-search-best-improvement",
            MetaheuristicAlgorithmIds.LocalSearchBestImprovement);
        Assert.Equal(
            "local-search-first-improvement",
            MetaheuristicAlgorithmIds.LocalSearchFirstImprovement);
    }

    private static INeighborhoodSearchInitialSolutionGenerator<int> Initial(int value) =>
        new DelegateNeighborhoodSearchInitialSolutionGenerator<int>((_, _) => value);

    private readonly struct StepMove
    {
        public StepMove(int delta) => Delta = delta;
        public int Delta { get; }
    }

    private struct LineEnumerator : INeighborhoodEnumerator<StepMove>
    {
        private int _index;

        public bool MoveNext(out StepMove move)
        {
            _index++;
            if (_index == 1)
            {
                move = new StepMove(-1);
                return true;
            }
            if (_index == 2)
            {
                move = new StepMove(1);
                return true;
            }
            move = default;
            return false;
        }
    }

    private sealed class LineNeighborhood : IEnumeratedNeighborhood<int, StepMove, LineEnumerator>
    {
        public LineEnumerator GetEnumerator(in int solution) => new();
    }

    private struct TwoStepEnumerator : INeighborhoodEnumerator<StepMove>
    {
        private int _index;

        public bool MoveNext(out StepMove move)
        {
            _index++;
            if (_index == 1)
            {
                move = new StepMove(1);
                return true;
            }
            if (_index == 2)
            {
                move = new StepMove(2);
                return true;
            }
            move = default;
            return false;
        }
    }

    private sealed class TwoStepNeighborhood : IEnumeratedNeighborhood<int, StepMove, TwoStepEnumerator>
    {
        public TwoStepEnumerator GetEnumerator(in int solution) => new();
    }

    private sealed class StepMoveOperator : IReversibleMoveOperator<int, StepMove, int>
    {
        public int ApplyCount { get; private set; }
        public int UndoCount { get; private set; }

        public int CaptureUndo(in int solution, in StepMove move) => solution;

        public void Apply(ref int solution, in StepMove move)
        {
            ApplyCount++;
            solution += move.Delta;
        }

        public void Undo(ref int solution, in StepMove move, in int undo)
        {
            UndoCount++;
            solution = undo;
        }
    }

    private sealed class QuadraticDelta : IMoveObjectiveDeltaEvaluator<int, StepMove>
    {
        private readonly int _target;
        public QuadraticDelta(int target) => _target = target;

        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in StepMove move,
            out double candidateObjective)
        {
            int x = solution + move.Delta;
            candidateObjective = (x - _target) * (double)(x - _target);
            return true;
        }
    }

    private sealed class QuadraticProblem : IOptimizationProblem<int>
    {
        private readonly int _target;
        public QuadraticProblem(int target) => _target = target;
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) =>
            (solution - _target) * (double)(solution - _target);
    }

    private sealed class PeakProblem : IOptimizationProblem<int>
    {
        private readonly int _target;
        public PeakProblem(int target) => _target = target;
        public OptimizationSense Sense => OptimizationSense.Maximize;
        public double Evaluate(int solution) =>
            -(solution - _target) * (double)(solution - _target);
    }

    private sealed class PeakDelta : IMoveObjectiveDeltaEvaluator<int, StepMove>
    {
        private readonly int _target;
        public PeakDelta(int target) => _target = target;

        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in StepMove move,
            out double candidateObjective)
        {
            int x = solution + move.Delta;
            candidateObjective = -(x - _target) * (double)(x - _target);
            return true;
        }
    }
}
