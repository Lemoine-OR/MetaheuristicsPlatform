using MetaheuristicsPlatform.Algorithms.Constructive;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class GraspPathRelinkingTests
{
    [Fact]
    public void ForwardRelinkingSelectsBestTargetDirectedMoveAndReachesGuide()
    {
        var problem =
            new PositiveIntMinimizationProblem();

        var procedure =
            CreateForwardProcedure();

        var context =
            new OptimizationContext<int>(
                TestDescriptor(),
                problem,
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100));

        context.Start();

        int initiating = 5;
        int guide = 1;
        double initiatingFitness =
            context.Evaluate(initiating);

        PathRelinkingProcedureResult<int> result =
            procedure.Relink(
                in initiating,
                initiatingFitness,
                in guide,
                context,
                new ImmutableSolutionCloner<int>(),
                maximumPathSteps: 10,
                TestContext.Current.CancellationToken);

        Assert.True(result.ReachedGuidingSolution);
        Assert.Equal(1, result.BestSolution);
        Assert.Equal(1.0, result.BestFitness);
        Assert.Equal(2, result.PathSteps);
        Assert.True(result.CandidateEvaluations >= 2);
        Assert.False(result.StoppingDecision.ShouldStop);
    }

    [Fact]
    public void ForwardRelinkingRejectsMoveThatDoesNotDecreaseGuideDistance()
    {
        var problem =
            new PositiveIntMinimizationProblem();

        var procedure =
            new GreedyForwardPathRelinkingProcedure<
                int,
                int,
                int,
                OneMoveEnumerator>(
                new InvalidAwayNeighborhood(),
                new IntDistance(),
                new IntMoveOperator(),
                new IntDeltaEvaluator());

        var context =
            new OptimizationContext<int>(
                TestDescriptor(),
                problem,
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100));

        context.Start();

        int initiating = 3;
        int guide = 1;
        double initiatingFitness =
            context.Evaluate(initiating);

        Assert.Throws<InvalidOperationException>(() =>
            procedure.Relink(
                in initiating,
                initiatingFitness,
                in guide,
                context,
                new ImmutableSolutionCloner<int>(),
                maximumPathSteps: 10,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ElitePoolRejectsNonDiverseCandidate()
    {
        var problem =
            new PositiveIntMinimizationProblem();

        var pool =
            new EliteSolutionPool<int>(
                capacity: 3,
                minimumDistance: 3,
                new IntDistance(),
                problem,
                new ImmutableSolutionCloner<int>());

        int first = 10;
        int tooClose = 12;

        Assert.True(pool.TryAdd(in first, 10.0, out _));
        Assert.False(pool.TryAdd(in tooClose, 12.0, out _));
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void ElitePoolReplacesWorstWhenBetterDiverseCandidateArrives()
    {
        var problem =
            new PositiveIntMinimizationProblem();

        var pool =
            new EliteSolutionPool<int>(
                capacity: 2,
                minimumDistance: 1,
                new IntDistance(),
                problem,
                new ImmutableSolutionCloner<int>());

        int ten = 10;
        int eight = 8;
        int seven = 7;

        Assert.True(pool.TryAdd(in ten, 10.0, out _));
        Assert.True(pool.TryAdd(in eight, 8.0, out _));

        Assert.True(
            pool.TryAdd(
                in seven,
                7.0,
                out bool replaced));

        Assert.True(replaced);
        Assert.Equal(2, pool.Count);

        Assert.True(
            pool.TrySelectGuide(
                in seven,
                new FixedRandomSource(),
                out int guide));

        Assert.Equal(8, guide);
    }

    [Fact]
    public void ParametersRejectInvalidEliteAndPathLimits()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                ElitePoolSize = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                MinimumEliteDistance = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                MaximumPathSteps = 0
            }.Validate());
    }

    [Fact]
    public void OptimizerUsesCommonOuterIterationStoppingLifecycle()
    {
        var optimizer =
            new GraspPathRelinkingOptimizer<int>(
                new SequenceConstructionProcedure(5, 3, 4),
                new NoOpLocalSearchProcedure(),
                CreateForwardProcedure(),
                new IntDistance());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new PositiveIntMinimizationProblem(),
                new GraspPathRelinkingParameters
                {
                    MaximumIterations = 20,
                    Alpha = 0.2,
                    ElitePoolSize = 3,
                    MinimumEliteDistance = 1,
                    MaximumPathSteps = 10
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(2),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("MaxIterations", result.StopDecision.Criterion);
        Assert.Equal("grasp-path-relinking", result.Algorithm.Id);
        Assert.Equal(3, result.BestSolution);
    }

    [Fact]
    public void StableIdAndRuntimeCatalogExposeGraspPathRelinking()
    {
        Assert.Equal(
            "grasp-path-relinking",
            MetaheuristicAlgorithmIds.GraspPathRelinking);

        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                MetaheuristicAlgorithmIds.GraspPathRelinking);

        Assert.Equal("constructive-methods", entry.Category);
        Assert.True(entry.RequiresComposition);
    }

    [Fact]
    public void DescriptorCarriesAiexEtAlPathRelinkingReference()
    {
        var optimizer =
            new GraspPathRelinkingOptimizer<int>(
                new SequenceConstructionProcedure(5),
                new NoOpLocalSearchProcedure(),
                CreateForwardProcedure(),
                new IntDistance());

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1287/ijoc.1030.0059");

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Hybrid));

        Assert.True(
            optimizer.Descriptor.Mechanisms.HasFlag(
                MetaheuristicMechanism.MemoryBased));
    }

    [Fact]
    public void AdvancedBackwardRelinkingUsesKnownEliteFitnessAndReachesOppositeEndpoint()
    {
        var problem = new PositiveIntMinimizationProblem();
        var procedure = CreateAdvancedProcedure();
        var context = new OptimizationContext<int>(
            TestDescriptor(),
            problem,
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100));

        context.Start();

        int initiating = 5;
        int guide = 1;
        double initiatingFitness = context.Evaluate(initiating);

        PathRelinkingProcedureResult<int> result =
            procedure.RelinkAdvanced(
                in initiating,
                initiatingFitness,
                in guide,
                guidingFitness: 1.0,
                new PathRelinkingExecutionOptions(
                    PathRelinkingDirectionStrategy.Backward,
                    PathRelinkingMoveSelectionStrategy.Greedy,
                    1.0,
                    0.2),
                context,
                new ImmutableSolutionCloner<int>(),
                maximumPathSteps: 20,
                TestContext.Current.CancellationToken);

        Assert.True(result.ReachedGuidingSolution);
        Assert.Equal(1, result.BestSolution);
        Assert.Equal(1.0, result.BestFitness);
    }

    [Fact]
    public void AdvancedBackAndForwardTraversesBothDirections()
    {
        var problem = new PositiveIntMinimizationProblem();
        var procedure = CreateAdvancedProcedure();
        var context = new OptimizationContext<int>(
            TestDescriptor(),
            problem,
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100));

        context.Start();

        int initiating = 5;
        int guide = 1;
        double initiatingFitness = context.Evaluate(initiating);

        PathRelinkingProcedureResult<int> result =
            procedure.RelinkAdvanced(
                in initiating,
                initiatingFitness,
                in guide,
                1.0,
                new PathRelinkingExecutionOptions(
                    PathRelinkingDirectionStrategy.BackAndForward,
                    PathRelinkingMoveSelectionStrategy.Greedy,
                    1.0,
                    0.2),
                context,
                new ImmutableSolutionCloner<int>(),
                20,
                TestContext.Current.CancellationToken);

        Assert.True(result.ReachedGuidingSolution);
        Assert.True(result.PathSteps > 2);
        Assert.Equal(1, result.BestSolution);
    }

    [Fact]
    public void AdvancedMixedRelinkingAlternatesEndpointsUntilTheyMeet()
    {
        var problem = new PositiveIntMinimizationProblem();
        var procedure = CreateAdvancedProcedure();
        var context = new OptimizationContext<int>(
            TestDescriptor(),
            problem,
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100));

        context.Start();

        int initiating = 5;
        int guide = 1;
        double initiatingFitness = context.Evaluate(initiating);

        PathRelinkingProcedureResult<int> result =
            procedure.RelinkAdvanced(
                in initiating,
                initiatingFitness,
                in guide,
                1.0,
                new PathRelinkingExecutionOptions(
                    PathRelinkingDirectionStrategy.Mixed,
                    PathRelinkingMoveSelectionStrategy.Greedy,
                    1.0,
                    0.2),
                context,
                new ImmutableSolutionCloner<int>(),
                20,
                TestContext.Current.CancellationToken);

        Assert.True(result.ReachedGuidingSolution);
        Assert.True(result.PathSteps >= 2);
        Assert.Equal(1, result.BestSolution);
    }

    [Fact]
    public void TruncatedPathRelinkingStopsAfterRequestedDistanceFraction()
    {
        var problem = new PositiveIntMinimizationProblem();
        var procedure = CreateAdvancedProcedure();
        var context = new OptimizationContext<int>(
            TestDescriptor(),
            problem,
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100));

        context.Start();

        int initiating = 5;
        int guide = 1;
        double initiatingFitness = context.Evaluate(initiating);

        PathRelinkingProcedureResult<int> result =
            procedure.RelinkAdvanced(
                in initiating,
                initiatingFitness,
                in guide,
                1.0,
                new PathRelinkingExecutionOptions(
                    PathRelinkingDirectionStrategy.Forward,
                    PathRelinkingMoveSelectionStrategy.Greedy,
                    0.5,
                    0.2),
                context,
                new ImmutableSolutionCloner<int>(),
                20,
                TestContext.Current.CancellationToken);

        Assert.False(result.ReachedGuidingSolution);
        Assert.Equal(1, result.PathSteps);
        Assert.Equal("PathRelinkingTruncatedFractionCompleted", result.StoppingDecision.Criterion);
    }

    [Fact]
    public void GreedyRandomizedAdaptivePathRelinkingUsesRclSampling()
    {
        var problem = new PositiveIntMinimizationProblem();
        var procedure = CreateAdvancedProcedure();
        var context = new OptimizationContext<int>(
            TestDescriptor(),
            problem,
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            new OptimizationOptions { Seed = 1UL });

        context.Start();

        int initiating = 5;
        int guide = 1;
        double initiatingFitness = context.Evaluate(initiating);

        PathRelinkingProcedureResult<int> result =
            procedure.RelinkAdvanced(
                in initiating,
                initiatingFitness,
                in guide,
                1.0,
                new PathRelinkingExecutionOptions(
                    PathRelinkingDirectionStrategy.Forward,
                    PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive,
                    1.0,
                    1.0),
                context,
                new ImmutableSolutionCloner<int>(),
                20,
                TestContext.Current.CancellationToken);

        Assert.True(result.ReachedGuidingSolution);
        Assert.True(result.CandidateEvaluations >= result.PathSteps);
    }

    [Fact]
    public void ElitePoolCanReturnStoredGuideFitnessWithoutReevaluation()
    {
        var pool = new EliteSolutionPool<int>(
            3,
            1,
            new IntDistance(),
            new PositiveIntMinimizationProblem(),
            new ImmutableSolutionCloner<int>());

        int elite = 7;
        int initiating = 10;
        Assert.True(pool.TryAdd(in elite, 7.0, out _));

        Assert.True(
            pool.TrySelectGuide(
                in initiating,
                new FixedRandomSource(),
                out int guide,
                out double guideFitness));

        Assert.Equal(7, guide);
        Assert.Equal(7.0, guideFitness);
    }

    [Fact]
    public void AdvancedParametersRejectInvalidPathFractionAndRclAlpha()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters { PathFraction = 0.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters { PathFraction = 1.01 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters { PathRelinkingAlpha = -0.01 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters { PathRelinkingAlpha = 1.01 }.Validate());
    }

    [Fact]
    public void EvolutionaryAdmissionReplacesClosestDominatedElite()
    {
        var pool = new EliteSolutionPool<int>(
            3,
            1,
            new IntDistance(),
            new PositiveIntMinimizationProblem(),
            new ImmutableSolutionCloner<int>());

        int ten = 10;
        int twenty = 20;
        int thirty = 30;
        int fifteen = 15;

        Assert.True(pool.TryAdd(in ten, 10.0, out _));
        Assert.True(pool.TryAdd(in twenty, 20.0, out _));
        Assert.True(pool.TryAdd(in thirty, 30.0, out _));

        Assert.True(
            pool.TryAddEvolutionary(
                in fifteen,
                15.0,
                out bool replaced));

        Assert.True(replaced);

        var values = new List<int>();

        for (int i = 0; i < pool.Count; i++)
        {
            pool.GetAt(i, out int solution, out _);
            values.Add(solution);
        }

        Assert.Contains(10, values);
        Assert.Contains(15, values);
        Assert.Contains(30, values);
        Assert.DoesNotContain(20, values);
    }

    [Fact]
    public void EvolutionaryAllPairsFindsInteriorImprovement()
    {
        var problem =
            new CenteredIntMinimizationProblem(center: 5);

        var cloner =
            new ImmutableSolutionCloner<int>();

        var context =
            new OptimizationContext<int>(
                TestDescriptor(),
                problem,
                cloner,
                new MaxEvaluationsStoppingCriterion(1000),
                new OptimizationOptions { Seed = 1UL });

        context.Start();

        var pool =
            new EliteSolutionPool<int>(
                3,
                1,
                new IntDistance(),
                problem,
                cloner);

        int one = 1;
        int nine = 9;
        int eleven = 11;

        Assert.True(
            pool.TryAdd(
                in one,
                context.Evaluate(one),
                out _));
        Assert.True(
            pool.TryAdd(
                in nine,
                context.Evaluate(nine),
                out _));
        Assert.True(
            pool.TryAdd(
                in eleven,
                context.Evaluate(eleven),
                out _));

        var pairwise =
            new AdvancedPathRelinkingProcedure<
                int,
                int,
                int,
                TowardGuideMoveEnumerator>(
                    new IntTowardGuideNeighborhood(),
                    new IntDistance(),
                    new IntMoveOperator());

        var evolutionary =
            new EvolutionaryPathRelinkingProcedure<int>(
                pairwise,
                new NoOpLocalSearchProcedure());

        EvolutionaryPathRelinkingResult<int> result =
            evolutionary.Evolve(
                pool,
                new PathRelinkingExecutionOptions(
                    PathRelinkingDirectionStrategy.Forward,
                    PathRelinkingMoveSelectionStrategy.Greedy,
                    1.0,
                    0.2),
                context,
                cloner,
                maximumGenerations: 5,
                maximumPathSteps: 20,
                improveOffspring: false,
                TestContext.Current.CancellationToken);

        Assert.Equal(5, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness);
        Assert.True(result.GenerationsCompleted >= 1);
        Assert.True(result.PairRelinkings >= 3);
        Assert.False(result.StoppingDecision.ShouldStop);
    }

    [Fact]
    public void OptimizerRunsEvolutionaryPathRelinkingPostOptimization()
    {
        var optimizer =
            new GraspPathRelinkingOptimizer<int>(
                new SequenceConstructionProcedure(1, 9, 11),
                new NoOpLocalSearchProcedure(),
                new AdvancedPathRelinkingProcedure<
                    int,
                    int,
                    int,
                    TowardGuideMoveEnumerator>(
                        new IntTowardGuideNeighborhood(),
                        new IntDistance(),
                        new IntMoveOperator()),
                new IntDistance());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new CenteredIntMinimizationProblem(center: 5),
                new GraspPathRelinkingParameters
                {
                    MaximumIterations = 3,
                    ElitePoolSize = 3,
                    MinimumEliteDistance = 1,
                    MaximumPathSteps = 1,
                    PathDirection = PathRelinkingDirectionStrategy.Forward,
                    PathMoveSelection = PathRelinkingMoveSelectionStrategy.Greedy,
                    PathFraction = 0.1,
                    EvolutionaryPathRelinkingEnabled = true,
                    MaximumEvolutionaryGenerations = 5,
                    MaximumEvolutionaryPathSteps = 20,
                    ImproveEvolutionaryOffspring = false,
                    EvolutionaryPathDirection = PathRelinkingDirectionStrategy.Forward,
                    EvolutionaryPathMoveSelection = PathRelinkingMoveSelectionStrategy.Greedy,
                    EvolutionaryPathFraction = 1.0
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(1000),
                new OptimizationOptions { Seed = 1UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "MaximumGraspPathRelinkingIterations",
            result.StopDecision.Criterion);
        Assert.Equal(5, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness);
    }

    [Fact]
    public void EvolutionaryParametersRejectInvalidGenerationAndPathLimits()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                MaximumEvolutionaryGenerations = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                MaximumEvolutionaryPathSteps = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                EvolutionaryPathFraction = 0.0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GraspPathRelinkingParameters
            {
                EvolutionaryPathRelinkingAlpha = 1.01
            }.Validate());
    }

    [Fact]
    public void DescriptorCarriesResendeWerneckEvolutionaryReference()
    {
        var optimizer =
            new GraspPathRelinkingOptimizer<int>(
                new SequenceConstructionProcedure(5),
                new NoOpLocalSearchProcedure(),
                CreateForwardProcedure(),
                new IntDistance());

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1023/B:HEUR.0000019986.96257.50");

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/j.cor.2008.05.011");
    }
    private static AdvancedPathRelinkingProcedure<
        int,
        int,
        int,
        TowardGuideMoveEnumerator> CreateAdvancedProcedure() =>
        new(
            new IntTowardGuideNeighborhood(),
            new IntDistance(),
            new IntMoveOperator(),
            new IntDeltaEvaluator());
    private static GreedyForwardPathRelinkingProcedure<
        int,
        int,
        int,
        TowardGuideMoveEnumerator> CreateForwardProcedure() =>
        new(
            new IntTowardGuideNeighborhood(),
            new IntDistance(),
            new IntMoveOperator(),
            new IntDeltaEvaluator());

    private static MetaheuristicDescriptor TestDescriptor() =>
        new()
        {
            Id = "test-path-relinking",
            Name = "Test Path Relinking",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution
        };

    private sealed class PositiveIntMinimizationProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            solution;
    }

    private sealed class CenteredIntMinimizationProblem :
        IOptimizationProblem<int>
    {
        private readonly int _center;

        public CenteredIntMinimizationProblem(int center)
        {
            _center = center;
        }

        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Math.Abs(solution - _center);
    }
    private sealed class IntDistance :
        IPathRelinkingDistance<int>
    {
        public int GetDistance(
            in int first,
            in int second,
            IOptimizationProblem<int> problem) =>
            Math.Abs(first - second);
    }

    private sealed class IntTowardGuideNeighborhood :
        IPathRelinkingNeighborhood<
            int,
            int,
            TowardGuideMoveEnumerator>
    {
        public TowardGuideMoveEnumerator GetEnumerator(
            in int current,
            in int guiding,
            IOptimizationProblem<int> problem) =>
            new(current, guiding);
    }

    private struct TowardGuideMoveEnumerator :
        INeighborhoodEnumerator<int>
    {
        private readonly int _direction;
        private readonly int _distance;
        private int _index;

        public TowardGuideMoveEnumerator(
            int current,
            int guiding)
        {
            _direction =
                Math.Sign(guiding - current);
            _distance =
                Math.Abs(guiding - current);
            _index = 0;
        }

        public bool MoveNext(out int move)
        {
            if (_direction == 0 ||
                _index >= Math.Min(2, _distance))
            {
                move = default;
                return false;
            }

            _index++;
            move =
                _direction * _index;
            return true;
        }
    }

    private sealed class InvalidAwayNeighborhood :
        IPathRelinkingNeighborhood<
            int,
            int,
            OneMoveEnumerator>
    {
        public OneMoveEnumerator GetEnumerator(
            in int current,
            in int guiding,
            IOptimizationProblem<int> problem) =>
            new(+1);
    }

    private struct OneMoveEnumerator :
        INeighborhoodEnumerator<int>
    {
        private readonly int _move;
        private bool _used;

        public OneMoveEnumerator(int move)
        {
            _move = move;
            _used = false;
        }

        public bool MoveNext(out int move)
        {
            if (_used)
            {
                move = default;
                return false;
            }

            _used = true;
            move = _move;
            return true;
        }
    }

    private sealed class IntMoveOperator :
        IReversibleMoveOperator<int, int, int>
    {
        public int CaptureUndo(
            in int solution,
            in int move) =>
            solution;

        public void Apply(
            ref int solution,
            in int move) =>
            solution += move;

        public void Undo(
            ref int solution,
            in int move,
            in int undo) =>
            solution = undo;
    }

    private sealed class IntDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<int, int>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in int move,
            out double candidateObjective)
        {
            candidateObjective =
                solution + move;
            return true;
        }
    }

    private sealed class SequenceConstructionProcedure :
        IGraspConstructionProcedure<int>
    {
        private readonly int[] _values;
        private int _index;

        public SequenceConstructionProcedure(
            params int[] values)
        {
            _values = values;
        }

        public GraspConstructionResult<int> Construct(
            IOptimizationProblem<int> problem,
            IRandomSource random,
            double alpha,
            int maximumConstructionSteps,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int value =
                _values[Math.Min(
                    _index,
                    _values.Length - 1)];

            _index++;

            return new GraspConstructionResult<int>(
                value,
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

    private sealed class FixedRandomSource :
        IRandomSource
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