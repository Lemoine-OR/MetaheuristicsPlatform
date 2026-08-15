using MetaheuristicsPlatform.Algorithms.TS;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class ReactiveTabuSearchAdvancedTests
{
    [Fact]
    public void RepetitionMemoryTreatsFirstVisitAsNew()
    {
        var memory = new ConfigurationRepetitionMemory();
        TabuSearchRepetitionObservation observation =
            memory.Observe(42UL, iteration: 0);

        Assert.False(observation.IsRepetition);
        Assert.Equal(1, observation.VisitCount);
        Assert.Equal(0, observation.CycleLength);
        Assert.Equal(1, memory.Count);
    }

    [Fact]
    public void RepetitionMemoryReportsCycleLengthAndVisitCount()
    {
        var memory = new ConfigurationRepetitionMemory();
        memory.Observe(7UL, iteration: 2);

        TabuSearchRepetitionObservation observation =
            memory.Observe(7UL, iteration: 9);

        Assert.True(observation.IsRepetition);
        Assert.Equal(2, observation.PreviousIteration);
        Assert.Equal(7, observation.CycleLength);
        Assert.Equal(2, observation.VisitCount);
    }

    [Fact]
    public void AttributeFrequencyMemoryCountsSelections()
    {
        var memory = new AttributeFrequencyMemory<int>();
        int attribute = 5;

        Assert.Equal(0, memory.GetFrequency(in attribute));
        Assert.Equal(1, memory.Record(in attribute));
        Assert.Equal(2, memory.Record(in attribute));
        Assert.Equal(2, memory.GetFrequency(in attribute));
        Assert.Equal(1, memory.Count);
    }

    [Fact]
    public void ReactiveTenureStartsAtConfiguredInitialValue()
    {
        var policy = new ReactiveTabuTenurePolicy(
            initialTenure: 3,
            minimumTenure: 1,
            maximumTenure: 20);

        Assert.Equal(3, policy.CurrentTenure);
    }

    [Fact]
    public void ReactiveTenureIncreasesWhenConfigurationRepeats()
    {
        var policy = new ReactiveTabuTenurePolicy(
            initialTenure: 2,
            minimumTenure: 1,
            maximumTenure: 20,
            increaseFactor: 1.5);

        TabuSearchRepetitionObservation repetition =
            new(
                isRepetition: true,
                previousIteration: 1,
                cycleLength: 4,
                visitCount: 2);

        var context = new ReactiveTabuTenureContext(
            iteration: 5,
            in repetition,
            currentObjective: 3.0,
            bestObjective: 2.0);

        ReactiveTabuReaction reaction =
            policy.Observe(in context);

        Assert.True(reaction.TenureChanged);
        Assert.Equal(3, reaction.TabuTenure);
        Assert.Equal(1, policy.RepetitionsObserved);
    }

    [Fact]
    public void ReactiveTenureDecreasesAfterRepetitionEvidenceDisappears()
    {
        var policy = new ReactiveTabuTenurePolicy(
            initialTenure: 4,
            minimumTenure: 1,
            maximumTenure: 20,
            increaseFactor: 1.5,
            decreaseFactor: 0.5,
            decreaseAfterIterationsWithoutRepetition: 3);

        TabuSearchRepetitionObservation repetition =
            new(true, 0, 2, 2);
        var repeatedContext =
            new ReactiveTabuTenureContext(
                2,
                in repetition,
                3.0,
                2.0);

        _ = policy.Observe(in repeatedContext);
        Assert.Equal(6, policy.CurrentTenure);

        TabuSearchRepetitionObservation fresh =
            new(false, -1, 0, 1);
        var quietContext =
            new ReactiveTabuTenureContext(
                5,
                in fresh,
                3.0,
                2.0);

        ReactiveTabuReaction reaction =
            policy.Observe(in quietContext);

        Assert.True(reaction.TenureChanged);
        Assert.Equal(3, reaction.TabuTenure);
    }

    [Fact]
    public void ReactiveTenureMaintainsMovingAverageCycleLength()
    {
        var policy = new ReactiveTabuTenurePolicy(
            cycleMovingAverageAlpha: 0.5,
            diversificationRepetitionThreshold: 100);

        TabuSearchRepetitionObservation first =
            new(true, 0, 4, 2);
        var firstContext =
            new ReactiveTabuTenureContext(
                4,
                in first,
                0.0,
                0.0);
        _ = policy.Observe(in firstContext);

        TabuSearchRepetitionObservation second =
            new(true, 4, 8, 3);
        var secondContext =
            new ReactiveTabuTenureContext(
                12,
                in second,
                0.0,
                0.0);
        _ = policy.Observe(in secondContext);

        Assert.Equal(
            6.0,
            policy.MovingAverageCycleLength,
            precision: 12);
    }

    [Fact]
    public void ReactiveTenureRequestsEscapeProportionalToCycleAverage()
    {
        var policy = new ReactiveTabuTenurePolicy(
            diversificationRepetitionThreshold: 2,
            diversificationCycleMultiplier: 1.5,
            cycleMovingAverageAlpha: 1.0);

        TabuSearchRepetitionObservation first =
            new(true, 0, 4, 2);
        var firstContext =
            new ReactiveTabuTenureContext(
                4,
                in first,
                0.0,
                0.0);
        ReactiveTabuReaction firstReaction =
            policy.Observe(in firstContext);

        Assert.False(firstReaction.DiversificationRequested);

        TabuSearchRepetitionObservation second =
            new(true, 4, 6, 3);
        var secondContext =
            new ReactiveTabuTenureContext(
                10,
                in second,
                0.0,
                0.0);
        ReactiveTabuReaction reaction =
            policy.Observe(in secondContext);

        Assert.True(reaction.DiversificationRequested);
        Assert.Equal(9, reaction.DiversificationMoves);
    }

    [Fact]
    public void ReactiveTenureSaturatesSafelyAtConfiguredMaximum()
    {
        var policy = new ReactiveTabuTenurePolicy(
            initialTenure: 2,
            minimumTenure: 1,
            maximumTenure: 7,
            increaseFactor: double.MaxValue,
            diversificationRepetitionThreshold: 100);

        TabuSearchRepetitionObservation repetition =
            new(true, 0, 1, 2);
        var context =
            new ReactiveTabuTenureContext(
                1,
                in repetition,
                0.0,
                0.0);

        ReactiveTabuReaction reaction =
            policy.Observe(in context);

        Assert.Equal(7, reaction.TabuTenure);
    }

    [Fact]
    public void ReactiveDiversificationLengthSaturatesSafelyAtConfiguredMaximum()
    {
        var policy = new ReactiveTabuTenurePolicy(
            diversificationRepetitionThreshold: 1,
            diversificationCycleMultiplier: double.MaxValue,
            maximumDiversificationMoves: 9);

        TabuSearchRepetitionObservation repetition =
            new(true, 0, 4, 2);
        var context =
            new ReactiveTabuTenureContext(
                4,
                in repetition,
                0.0,
                0.0);

        ReactiveTabuReaction reaction =
            policy.Observe(in context);

        Assert.True(reaction.DiversificationRequested);
        Assert.Equal(9, reaction.DiversificationMoves);
    }

    [Fact]
    public void AcknowledgingDiversificationResetsReactiveTriggerCount()
    {
        var policy = new ReactiveTabuTenurePolicy(
            diversificationRepetitionThreshold: 1);

        TabuSearchRepetitionObservation repetition =
            new(true, 0, 2, 2);
        var context =
            new ReactiveTabuTenureContext(
                2,
                in repetition,
                0.0,
                0.0);

        Assert.True(
            policy.Observe(in context)
                .DiversificationRequested);

        policy.AcknowledgeDiversification();

        TabuSearchRepetitionObservation fresh =
            new(false, -1, 0, 1);
        var freshContext =
            new ReactiveTabuTenureContext(
                3,
                in fresh,
                0.0,
                0.0);

        Assert.False(
            policy.Observe(in freshContext)
                .DiversificationRequested);
    }

    [Fact]
    public void ReactiveParametersRejectInvalidTenureBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ReactiveTabuSearchParameters
            {
                InitialTabuTenure = 5,
                MinimumTabuTenure = 1,
                MaximumTabuTenure = 4
            }.Validate());
    }

    [Fact]
    public void ReactiveParametersRejectInvalidFeedbackFactors()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ReactiveTabuSearchParameters
            {
                TenureIncreaseFactor = 1.0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ReactiveTabuSearchParameters
            {
                TenureDecreaseFactor = 1.0
            }.Validate());
    }

    [Fact]
    public void TabuComponentCatalogHasAtLeastTenUniqueExecutableIds()
    {
        Assert.True(
            TabuSearchComponentCatalog.All.Count >= 10);

        Assert.Equal(
            TabuSearchComponentCatalog.All.Count,
            TabuSearchComponentCatalog.All
                .Select(static entry => entry.Id)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void ReactiveComponentCatalogResolvesCanonicalTenureId()
    {
        TabuSearchComponentDescriptor descriptor =
            TabuSearchComponentCatalog.GetRequired(
                TabuSearchComponentIds.ReactiveTenure);

        Assert.Equal(
            "ReactiveTabuTenurePolicy",
            descriptor.ImplementationType);
    }

    [Fact]
    public void ReactiveAlgorithmCatalogExposesStablePublicId()
    {
        var entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.ReactiveTabuSearch);

        Assert.Equal(
            "reactive-tabu-search-battiti-tecchiolli-1994",
            entry.Id);
        Assert.True(entry.RequiresComposition);
        Assert.Equal(
            "10.1287/ijoc.6.2.126",
            entry.Doi);
    }

    [Fact]
    public void ReactiveDescriptorIsAdaptiveMemoryBasedTrajectoryMethod()
    {
        var optimizer =
            CreateOptimizer(
                new CountingMoveOperator(),
                new NeverTabuMemory<int>());

        Assert.Equal(
            MetaheuristicAlgorithmIds.ReactiveTabuSearch,
            optimizer.Descriptor.Id);

        Assert.True(
            (optimizer.Descriptor.Mechanisms &
             MetaheuristicsPlatform.Classification.MetaheuristicMechanism.Adaptive) != 0);

        Assert.True(
            (optimizer.Descriptor.Mechanisms &
             MetaheuristicsPlatform.Classification.MetaheuristicMechanism.MemoryBased) != 0);
    }

    [Fact]
    public void ReactiveExactDeltaPathAppliesOnlySelectedMoveInNormalIteration()
    {
        var moveOperator =
            new CountingMoveOperator();

        var optimizer =
            CreateOptimizer(
                moveOperator,
                new NeverTabuMemory<int>());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new QuadraticProblem(target: 2),
                new ReactiveTabuSearchParameters
                {
                    DiversificationRepetitionThreshold = 100
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(1),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(1, moveOperator.ApplyCount);
        Assert.Equal(0, moveOperator.UndoCount);
        Assert.Equal(1, result.Statistics.Iterations);
        Assert.Equal(3, result.Statistics.Evaluations);
    }

    [Fact]
    public void ReactiveEvaluationBudgetStopsInsideNormalNeighborhoodScanWithoutOvershoot()
    {
        var moveOperator =
            new CountingMoveOperator();

        var optimizer =
            CreateOptimizer(
                moveOperator,
                new NeverTabuMemory<int>());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new QuadraticProblem(target: 2),
                new ReactiveTabuSearchParameters
                {
                    DiversificationRepetitionThreshold = 100
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(2),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal("MaxEvaluations", result.StopDecision.Criterion);
        Assert.Equal(2, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
        Assert.Equal(0, moveOperator.ApplyCount);
    }

    [Fact]
    public void DefaultOptionalLongTermControlsStayOff()
    {
        ReactiveTabuSearchState? finalState = null;

        var callback =
            new DelegateOptimizationCallback<int>(
                (in OptimizationEvent<int> e) =>
                {
                    if (e.AlgorithmData is ReactiveTabuSearchState state)
                    {
                        finalState = state;
                    }
                });

        var optimizer =
            CreateOptimizer(
                new CountingMoveOperator(),
                new NeverTabuMemory<int>());

        _ = optimizer.Optimize(
            new QuadraticProblem(target: 2),
            new ReactiveTabuSearchParameters
            {
                DiversificationRepetitionThreshold = 100
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            callback: callback,
            cancellationToken:
                TestContext.Current.CancellationToken);

        Assert.True(finalState.HasValue);
        Assert.Equal(0, finalState.Value.FrequencyTrackedAttributes);
        Assert.Equal(0, finalState.Value.IntensificationRestarts);
    }

    [Fact]
    public void ReactiveFullEvaluationFallbackUsesApplyEvaluateUndoThenAppliesWinner()
    {
        var moveOperator =
            new CountingMoveOperator();

        var optimizer =
            new ReactiveTabuSearchOptimizer<
                int,
                StepMove,
                int,
                int,
                LineEnumerator>(
                new DelegateTabuSearchInitialSolutionGenerator<int>(
                    (_, _) => 0),
                new LineNeighborhood(),
                moveOperator,
                new LineAttributeProvider(),
                new DelegateTabuSearchSolutionSignatureProvider<int>(
                    (in int value) => unchecked((ulong)(long)value)),
                deltaEvaluator: null,
                memoryFactory: _ => new NeverTabuMemory<int>());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new QuadraticProblem(target: 2),
                new ReactiveTabuSearchParameters
                {
                    DiversificationRepetitionThreshold = 100
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(1),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(3, moveOperator.ApplyCount);
        Assert.Equal(2, moveOperator.UndoCount);
        Assert.Equal(3, result.Statistics.Evaluations);
    }

    [Fact]
    public void ReactiveSearchDetectsRepeatedConfigurationAndRaisesTenure()
    {
        ReactiveTabuSearchState? finalState = null;

        var callback =
            new DelegateOptimizationCallback<int>(
                (in OptimizationEvent<int> e) =>
                {
                    if (e.AlgorithmData is ReactiveTabuSearchState state)
                    {
                        finalState = state;
                    }
                });

        var optimizer =
            CreateOptimizer(
                new CountingMoveOperator(),
                new NeverTabuMemory<int>(),
                initialValue: 0,
                deltaEvaluator: new AbsoluteValueDeltaEvaluator());

        _ = optimizer.Optimize(
            new AbsoluteValueProblem(),
            new ReactiveTabuSearchParameters
            {
                InitialTabuTenure = 1,
                DiversificationRepetitionThreshold = 100
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(2),
            callback: callback,
            cancellationToken:
                TestContext.Current.CancellationToken);

        Assert.True(finalState.HasValue);
        Assert.True(
            finalState.Value.RepeatedConfigurations >= 1);
        Assert.True(
            finalState.Value.CurrentTabuTenure > 1);
    }

    [Fact]
    public void PersistentRepetitionActivatesRandomWalkDiversification()
    {
        ReactiveTabuSearchState? finalState = null;

        var callback =
            new DelegateOptimizationCallback<int>(
                (in OptimizationEvent<int> e) =>
                {
                    if (e.AlgorithmData is ReactiveTabuSearchState state)
                    {
                        finalState = state;
                    }
                });

        var optimizer =
            CreateOptimizer(
                new CountingMoveOperator(),
                new NeverTabuMemory<int>(),
                initialValue: 0,
                deltaEvaluator: new AbsoluteValueDeltaEvaluator());

        _ = optimizer.Optimize(
            new AbsoluteValueProblem(),
            new ReactiveTabuSearchParameters
            {
                DiversificationRepetitionThreshold = 1,
                DiversificationCycleMultiplier = 1.0,
                MaximumDiversificationMoves = 10
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(3),
            options: new OptimizationOptions { Seed = 123UL },
            callback: callback,
            cancellationToken:
                TestContext.Current.CancellationToken);

        Assert.True(finalState.HasValue);
        Assert.True(
            finalState.Value.DiversificationPhases >= 1);
        Assert.True(
            finalState.Value.DiversificationMoves >= 1);
    }

    [Fact]
    public void EliteIntensificationRestartsAfterConfiguredStagnation()
    {
        ReactiveTabuSearchState? finalState = null;

        var callback =
            new DelegateOptimizationCallback<int>(
                (in OptimizationEvent<int> e) =>
                {
                    if (e.AlgorithmData is ReactiveTabuSearchState state)
                    {
                        finalState = state;
                    }
                });

        var optimizer =
            CreateOptimizer(
                new CountingMoveOperator(),
                new NeverTabuMemory<int>(),
                initialValue: 0,
                deltaEvaluator: new AbsoluteValueDeltaEvaluator());

        _ = optimizer.Optimize(
            new AbsoluteValueProblem(),
            new ReactiveTabuSearchParameters
            {
                IntensificationAfterIterationsWithoutImprovement = 1,
                DiversificationRepetitionThreshold = 100
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            callback: callback,
            cancellationToken:
                TestContext.Current.CancellationToken);

        Assert.True(finalState.HasValue);
        Assert.Equal(
            1,
            finalState.Value.IntensificationRestarts);
        Assert.Equal(
            0.0,
            finalState.Value.CurrentObjective,
            precision: 12);
    }

    [Fact]
    public void FrequencyPenaltyBreaksEqualObjectiveTieTowardLessVisitedAttribute()
    {
        var moveOperator =
            new CountingMoveOperator();

        var optimizer =
            CreateOptimizer(
                moveOperator,
                new NeverTabuMemory<int>(),
                initialValue: 0,
                deltaEvaluator: new AbsoluteValueDeltaEvaluator());

        _ = optimizer.Optimize(
            new AbsoluteValueProblem(),
            new ReactiveTabuSearchParameters
            {
                FrequencyPenaltyWeight = 10.0,
                DiversificationRepetitionThreshold = 100
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(3),
            cancellationToken:
                TestContext.Current.CancellationToken);

        Assert.Equal(
            1,
            moveOperator.LastAppliedDelta);
    }

    private static ReactiveTabuSearchOptimizer<
        int,
        StepMove,
        int,
        int,
        LineEnumerator> CreateOptimizer(
            CountingMoveOperator moveOperator,
            ITabuMemory<int> memory,
            int initialValue = 0,
            IMoveObjectiveDeltaEvaluator<int, StepMove>? deltaEvaluator = null) =>
        new(
            new DelegateTabuSearchInitialSolutionGenerator<int>(
                (_, _) => initialValue),
            new LineNeighborhood(),
            moveOperator,
            new LineAttributeProvider(),
            new DelegateTabuSearchSolutionSignatureProvider<int>(
                (in int value) =>
                    unchecked((ulong)(long)value)),
            deltaEvaluator ?? new QuadraticDeltaEvaluator(target: 2),
            memoryFactory: _ => memory);

    private readonly struct StepMove
    {
        public StepMove(int delta)
        {
            Delta = delta;
        }

        public int Delta { get; }
    }

    private struct LineEnumerator :
        INeighborhoodEnumerator<StepMove>
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
        IEnumeratedNeighborhood<
            int,
            StepMove,
            LineEnumerator>
    {
        public LineEnumerator GetEnumerator(
            in int solution) =>
            new();
    }

    private sealed class CountingMoveOperator :
        IReversibleMoveOperator<
            int,
            StepMove,
            int>
    {
        public int ApplyCount { get; private set; }
        public int UndoCount { get; private set; }
        public int LastAppliedDelta { get; private set; }

        public int CaptureUndo(
            in int solution,
            in StepMove move) =>
            solution;

        public void Apply(
            ref int solution,
            in StepMove move)
        {
            ApplyCount++;
            LastAppliedDelta = move.Delta;
            solution += move.Delta;
        }

        public void Undo(
            ref int solution,
            in StepMove move,
            in int undo)
        {
            UndoCount++;
            solution = undo;
        }
    }

    private sealed class LineAttributeProvider :
        ITabuAttributeProvider<
            int,
            StepMove,
            int>
    {
        public int GetCandidateAttribute(
            in int solution,
            in StepMove move) =>
            solution + move.Delta;

        public int GetAttributeToForbid(
            in int solution,
            in StepMove move) =>
            solution;
    }

    private sealed class QuadraticProblem :
        IOptimizationProblem<int>
    {
        private readonly int _target;

        public QuadraticProblem(int target)
        {
            _target = target;
        }

        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            (solution - _target) *
            (double)(solution - _target);
    }

    private sealed class AbsoluteValueProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Math.Abs(solution);
    }

    private sealed class QuadraticDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<
            int,
            StepMove>
    {
        private readonly int _target;

        public QuadraticDeltaEvaluator(int target)
        {
            _target = target;
        }

        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in StepMove move,
            out double candidateObjective)
        {
            int candidate =
                solution + move.Delta;

            candidateObjective =
                (candidate - _target) *
                (double)(candidate - _target);

            return true;
        }
    }

    private sealed class AbsoluteValueDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<
            int,
            StepMove>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in StepMove move,
            out double candidateObjective)
        {
            candidateObjective =
                Math.Abs(solution + move.Delta);

            return true;
        }
    }

    private sealed class NeverTabuMemory<T> :
        ITabuMemory<T>
        where T : notnull
    {
        public int Count => 0;

        public void Advance(long iteration)
        {
        }

        public bool IsTabu(
            in T attribute,
            long iteration) =>
            false;

        public void Register(
            in T attribute,
            long tabuUntilIteration)
        {
        }
    }
}
