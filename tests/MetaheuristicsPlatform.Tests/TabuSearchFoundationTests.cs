using MetaheuristicsPlatform.Algorithms.TS;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class TabuSearchFoundationTests
{
    [Fact]
    public void ExpirationMemoryHonorsInclusiveTenureBoundary()
    {
        var memory = new ExpirationTabuMemory<int>();
        int attribute = 7;
        memory.Register(in attribute, tabuUntilIteration: 3);

        Assert.True(memory.IsTabu(in attribute, iteration: 1));
        Assert.True(memory.IsTabu(in attribute, iteration: 3));

        memory.Advance(iteration: 4);
        Assert.False(memory.IsTabu(in attribute, iteration: 4));
        Assert.Equal(0, memory.Count);
    }

    [Fact]
    public void ExpirationMemoryCleansNonMonotonicVaryingTenuresInExpirationOrder()
    {
        var memory = new ExpirationTabuMemory<int>();
        int longLived = 1;
        int shortLived = 2;

        memory.Register(in longLived, tabuUntilIteration: 10);
        memory.Register(in shortLived, tabuUntilIteration: 5);

        memory.Advance(iteration: 6);

        Assert.True(memory.IsTabu(in longLived, iteration: 6));
        Assert.False(memory.IsTabu(in shortLived, iteration: 6));
        Assert.Equal(1, memory.Count);
    }

    [Fact]
    public void ExpirationMemoryIgnoresStalePriorityRecordAfterReregistration()
    {
        var memory = new ExpirationTabuMemory<int>();
        int attribute = 5;
        memory.Register(in attribute, tabuUntilIteration: 2);
        memory.Register(in attribute, tabuUntilIteration: 5);

        memory.Advance(iteration: 3);

        Assert.True(memory.IsTabu(in attribute, iteration: 3));
        Assert.Equal(1, memory.Count);
    }

    [Fact]
    public void BestSoFarAspirationUsesOptimizationSense()
    {
        var criterion = new BestSoFarAspirationCriterion();
        var minimize = new TabuAspirationContext(1, 1, 10.0, 5.0, 4.0);
        var maximize = new TabuAspirationContext(1, 1, 10.0, 15.0, 16.0);

        Assert.True(criterion.IsAspirational(in minimize, OptimizationSense.Minimize));
        Assert.True(criterion.IsAspirational(in maximize, OptimizationSense.Maximize));
    }

    [Fact]
    public void NoAspirationEnablesPreEvaluationTabuRejection()
    {
        var criterion = new NoTabuAspirationCriterion();
        Assert.False(criterion.RequiresCandidateObjective);
    }

    [Fact]
    public void FixedTenureReturnsConfiguredValue()
    {
        var policy = new FixedTabuTenurePolicy(9);
        var context = new TabuTenureContext(1, 3.0, 2.0, 2.0, 2, 0, 0);
        var random = new Xoshiro256StarStarRandomSource(123UL);

        Assert.Equal(9, policy.GetTenure(in context, random));
    }

    [Fact]
    public void UniformRandomTenureStaysInsideInclusiveBounds()
    {
        var policy = new UniformRandomTabuTenurePolicy(3, 6);
        var context = new TabuTenureContext(1, 3.0, 2.0, 2.0, 2, 0, 0);
        var random = new Xoshiro256StarStarRandomSource(123UL);

        for (int i = 0; i < 100; i++)
        {
            int tenure = policy.GetTenure(in context, random);
            Assert.InRange(tenure, 3, 6);
        }
    }

    [Fact]
    public void ParametersRejectInvalidTenures()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TabuSearchParameters { FixedTabuTenure = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TabuSearchParameters
            {
                RandomTenureMinimum = 5,
                RandomTenureMaximum = 4
            }.Validate());
    }

    [Fact]
    public void ExactDeltaPathAppliesOnlySelectedMove()
    {
        var moveOperator = new CountingMoveOperator();
        var optimizer = CreateOptimizer(
            moveOperator,
            deltaEvaluator: new QuadraticDeltaEvaluator(),
            memoryFactory: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(target: 2),
            new TabuSearchParameters { FixedTabuTenure = 2 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, moveOperator.ApplyCount);
        Assert.Equal(0, moveOperator.UndoCount);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void EvaluationBudgetStopsInsideNeighborhoodScanWithoutOvershoot()
    {
        var moveOperator = new CountingMoveOperator();
        var optimizer = CreateOptimizer(
            moveOperator,
            deltaEvaluator: new QuadraticDeltaEvaluator(),
            memoryFactory: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(target: 2),
            new TabuSearchParameters { FixedTabuTenure = 2 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("MaxEvaluations", result.StopDecision.Criterion);
        Assert.Equal(2, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
        Assert.Equal(0, moveOperator.ApplyCount);
    }

    [Fact]
    public void FullEvaluationPathUsesApplyUndoForCandidatesThenAppliesWinnerOnce()
    {
        var moveOperator = new CountingMoveOperator();
        var optimizer = CreateOptimizer(
            moveOperator,
            deltaEvaluator: null,
            memoryFactory: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(target: 2),
            new TabuSearchParameters { FixedTabuTenure = 2 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, moveOperator.ApplyCount);
        Assert.Equal(2, moveOperator.UndoCount);
        Assert.Equal(3, result.Statistics.Evaluations);
    }

    [Fact]
    public void BestAdmissibleSelectionCanMoveThroughWorseningSolution()
    {
        var optimizer = CreateOptimizer(
            new CountingMoveOperator(),
            deltaEvaluator: new LandscapeDeltaEvaluator(),
            memoryFactory: null,
            initialValue: 1);

        OptimizationResult<int> result = optimizer.Optimize(
            new LandscapeProblem(),
            new TabuSearchParameters { FixedTabuTenure = 2 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(2),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(-1.0, result.BestFitness, precision: 12);
        Assert.Equal(3, result.BestSolution);
    }

    [Fact]
    public void BestSoFarAspirationReleasesImprovingTabuCandidate()
    {
        var optimizer = CreateOptimizer(
            new CountingMoveOperator(),
            deltaEvaluator: new QuadraticDeltaEvaluator(),
            memoryFactory: _ => new AlwaysTabuMemory<int>());

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(target: 2),
            new TabuSearchParameters
            {
                FixedTabuTenure = 2,
                AspirationCriterionKind = TabuAspirationCriterionKind.BestSoFar
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.BestFitness < 4.0);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void DisabledAspirationSkipsObjectiveEvaluationForTabuCandidate()
    {
        var optimizer = CreateOptimizer(
            new CountingMoveOperator(),
            deltaEvaluator: new QuadraticDeltaEvaluator(),
            memoryFactory: _ => new AlwaysTabuMemory<int>());

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(target: 2),
            new TabuSearchParameters
            {
                FixedTabuTenure = 2,
                AspirationCriterionKind = TabuAspirationCriterionKind.None
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(10),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("NoAdmissibleMove", result.StopDecision.Criterion);
        Assert.Equal(1, result.Statistics.Evaluations);
    }

    [Fact]
    public void SearchReachesBestAlongSimpleLine()
    {
        var optimizer = CreateOptimizer(
            new CountingMoveOperator(),
            deltaEvaluator: new QuadraticDeltaEvaluator(target: 3),
            memoryFactory: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(target: 3),
            new TabuSearchParameters { FixedTabuTenure = 2 },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(3),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0.0, result.BestFitness, precision: 12);
        Assert.Equal(3, result.BestSolution);
    }

    [Fact]
    public void DescriptorClassifiesTabuSearchAsMemoryBasedTrajectoryMethod()
    {
        var optimizer = CreateOptimizer(
            new CountingMoveOperator(),
            deltaEvaluator: new QuadraticDeltaEvaluator(),
            memoryFactory: null);

        Assert.Equal("tabu-search-glover", optimizer.Descriptor.Id);
        Assert.True((optimizer.Descriptor.Mechanisms &
                     MetaheuristicsPlatform.Classification.MetaheuristicMechanism.MemoryBased) != 0);
    }

    [Fact]
    public void CatalogExposesStableTabuSearchId()
    {
        var entry = MetaheuristicCatalog.GetRequired(MetaheuristicAlgorithmIds.TabuSearch);
        Assert.Equal("tabu-search-glover", entry.Id);
        Assert.True(entry.RequiresComposition);
    }

    private static TabuSearchOptimizer<int, StepMove, int, int, LineEnumerator>
        CreateOptimizer(
            CountingMoveOperator moveOperator,
            IMoveObjectiveDeltaEvaluator<int, StepMove>? deltaEvaluator,
            Func<int, ITabuMemory<int>>? memoryFactory,
            int initialValue = 0) =>
        new(
            new DelegateTabuSearchInitialSolutionGenerator<int>((_, _) => initialValue),
            new LineNeighborhood(),
            moveOperator,
            new LineAttributeProvider(),
            deltaEvaluator,
            memoryFactory: memoryFactory);

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

    private sealed class LineNeighborhood :
        IEnumeratedNeighborhood<int, StepMove, LineEnumerator>
    {
        public LineEnumerator GetEnumerator(in int solution) => new();
    }

    private sealed class CountingMoveOperator :
        IReversibleMoveOperator<int, StepMove, int>
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

    private sealed class LineAttributeProvider :
        ITabuAttributeProvider<int, StepMove, int>
    {
        public int GetCandidateAttribute(in int solution, in StepMove move) =>
            solution + move.Delta;

        public int GetAttributeToForbid(in int solution, in StepMove move) =>
            solution;
    }

    private sealed class QuadraticProblem : IOptimizationProblem<int>
    {
        private readonly int _target;
        public QuadraticProblem(int target) => _target = target;
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) =>
            (solution - _target) * (double)(solution - _target);
    }

    private sealed class QuadraticDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<int, StepMove>
    {
        private readonly int _target;

        public QuadraticDeltaEvaluator(int target = 2) => _target = target;

        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in StepMove move,
            out double candidateObjective)
        {
            int candidate = solution + move.Delta;
            candidateObjective = (candidate - _target) * (double)(candidate - _target);
            return true;
        }
    }

    private sealed class LandscapeProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) => Landscape(solution);
        public static double Landscape(int x) => x switch
        {
            -1 => 4.0,
            0 => 3.0,
            1 => 0.0,
            2 => 2.0,
            3 => -1.0,
            _ => 10.0 + Math.Abs(x)
        };
    }

    private sealed class LandscapeDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<int, StepMove>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in StepMove move,
            out double candidateObjective)
        {
            candidateObjective = LandscapeProblem.Landscape(solution + move.Delta);
            return true;
        }
    }

    private sealed class AlwaysTabuMemory<T> : ITabuMemory<T>
        where T : notnull
    {
        public int Count => 1;
        public void Advance(long iteration) { }
        public bool IsTabu(in T attribute, long iteration) => true;
        public void Register(in T attribute, long tabuUntilIteration) { }
    }
}
