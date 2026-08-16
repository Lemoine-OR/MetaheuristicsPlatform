using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class GuidedLocalSearchTests
{
    [Fact]
    public void GlsPenalizationEscapesOriginalLocalOptimum()
    {
        var optimizer = CreateOptimizer(
            new SingleFeatureModel(),
            penaltyDeltaEvaluator: null,
            objectiveDeltaEvaluator: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new PiecewiseMinProblem(),
            new GuidedLocalSearchParameters
            {
                PenaltyWeight = 2.0,
                MaximumPenaltyUpdates = 1,
                SelectionPolicy = LocalSearchSelectionPolicy.BestImprovement
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal(
            "MaximumGuidedPenaltyUpdates",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void GlsPenalizesAllMaximumUtilityTies()
    {
        var optimizer = CreateOptimizer(
            new TiedFeatureModel(),
            penaltyDeltaEvaluator: null,
            objectiveDeltaEvaluator: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new PiecewiseMinProblem(),
            new GuidedLocalSearchParameters
            {
                PenaltyWeight = 1.0,
                MaximumPenaltyUpdates = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        // At x=0, two active features have equal maximal utility.
        // Penalizing both raises h(0) from 5 to 7, enabling x=1 (h=6)
        // and subsequently the global optimum x=2.
        Assert.Equal(2, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void GlsGeneralizesPenaltyDirectionToMaximization()
    {
        var optimizer = CreateOptimizer(
            new SingleFeatureModel(),
            penaltyDeltaEvaluator: null,
            objectiveDeltaEvaluator: null);

        OptimizationResult<int> result = optimizer.Optimize(
            new PiecewiseMaxProblem(),
            new GuidedLocalSearchParameters
            {
                PenaltyWeight = 2.0,
                MaximumPenaltyUpdates = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.BestSolution);
        Assert.Equal(10.0, result.BestFitness, 12);
    }

    [Fact]
    public void ExactObjectiveAndPenaltyDeltasAvoidFullCandidateEvaluation()
    {
        var problem = new CountingPiecewiseMinProblem();
        var optimizer = CreateOptimizer(
            new SingleFeatureModel(),
            new SingleFeaturePenaltyDeltaEvaluator(),
            new PiecewiseObjectiveDeltaEvaluator());

        OptimizationResult<int> result = optimizer.Optimize(
            problem,
            new GuidedLocalSearchParameters
            {
                PenaltyWeight = 2.0,
                MaximumPenaltyUpdates = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.BestSolution);
        Assert.Equal(1, problem.FullEvaluations);
        Assert.True(result.Statistics.Evaluations > problem.FullEvaluations);
    }

    [Fact]
    public void GlsParametersRejectInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GuidedLocalSearchParameters
            {
                PenaltyWeight = 0.0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GuidedLocalSearchParameters
            {
                MaximumPenaltyUpdates = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GuidedLocalSearchParameters
            {
                MaximumAcceptedMovesPerPenaltyPhase = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GuidedLocalSearchParameters
            {
                SelectionPolicy = (LocalSearchSelectionPolicy)999
            }.Validate());
    }

    [Fact]
    public void CatalogContainsGuidedLocalSearchStableId()
    {
        MetaheuristicCatalogEntry entry =
            MetaheuristicCatalog.GetRequired(
                "guided-local-search-voudouris-tsang-1999");

        Assert.Equal(
            "guided-local-search-voudouris-tsang-1999",
            entry.Id);
        Assert.True(entry.RequiresComposition);
    }

    [Fact]
    public void PublicAlgorithmIdsExposeGuidedLocalSearch()
    {
        Assert.Equal(
            "guided-local-search-voudouris-tsang-1999",
            MetaheuristicAlgorithmIds.GuidedLocalSearch);
    }

    [Fact]
    public void DescriptorCarriesCanonicalGuidedLocalSearchReferences()
    {
        var optimizer = CreateOptimizer(
            new SingleFeatureModel(),
            penaltyDeltaEvaluator: null,
            objectiveDeltaEvaluator: null);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1016/S0167-6377(96)00042-9");

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1016/S0377-2217(98)00099-X");
    }

    private static GuidedLocalSearchOptimizer<
        int,
        int,
        int,
        StepEnumerator,
        int,
        SingleFeatureEnumerator> CreateOptimizer(
            IGuidedLocalSearchFeatureModel<
                int,
                int,
                SingleFeatureEnumerator> featureModel,
            IGuidedLocalSearchPenaltyDeltaEvaluator<
                int,
                int,
                int>? penaltyDeltaEvaluator,
            IMoveObjectiveDeltaEvaluator<int, int>? objectiveDeltaEvaluator) =>
        new(
            new DelegateNeighborhoodSearchInitialSolutionGenerator<int>(
                static (_, _) => 0),
            new StepNeighborhood(),
            new IntMoveOperator(),
            featureModel,
            objectiveDeltaEvaluator,
            penaltyDeltaEvaluator,
            new BoundsApplicability());

    private sealed class StepNeighborhood :
        IEnumeratedNeighborhood<int, int, StepEnumerator>
    {
        public StepEnumerator GetEnumerator(in int solution) => new();
    }

    private struct StepEnumerator : INeighborhoodEnumerator<int>
    {
        private int _index;

        public bool MoveNext(out int move)
        {
            if (_index == 0)
            {
                _index++;
                move = -1;
                return true;
            }

            if (_index == 1)
            {
                _index++;
                move = 1;
                return true;
            }

            move = default;
            return false;
        }
    }

    private sealed class BoundsApplicability :
        IMoveApplicability<int, int>
    {
        public bool IsApplicable(
            in int solution,
            in int move)
        {
            int candidate = solution + move;
            return candidate >= 0 && candidate <= 2;
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

    private sealed class SingleFeatureModel :
        IGuidedLocalSearchFeatureModel<
            int,
            int,
            SingleFeatureEnumerator>
    {
        public SingleFeatureEnumerator GetEnumerator(
            in int solution) =>
            new(FeatureFor(solution));

        public double GetFeatureCost(
            in int solution,
            in int feature) =>
            feature == 0 ? 10.0 : 1.0;
    }

    private sealed class TiedFeatureModel :
        IGuidedLocalSearchFeatureModel<
            int,
            int,
            SingleFeatureEnumerator>
    {
        public SingleFeatureEnumerator GetEnumerator(
            in int solution) =>
            solution == 0
                ? new SingleFeatureEnumerator(0, 1)
                : new SingleFeatureEnumerator(2);

        public double GetFeatureCost(
            in int solution,
            in int feature) =>
            feature is 0 or 1 ? 10.0 : 1.0;
    }

    private struct SingleFeatureEnumerator :
        IGuidedLocalSearchFeatureEnumerator<int>
    {
        private readonly int _first;
        private readonly int _second;
        private readonly int _count;
        private int _index;

        public SingleFeatureEnumerator(int first)
        {
            _first = first;
            _second = default;
            _count = 1;
            _index = 0;
        }

        public SingleFeatureEnumerator(
            int first,
            int second)
        {
            _first = first;
            _second = second;
            _count = 2;
            _index = 0;
        }

        public bool MoveNext(out int feature)
        {
            if (_index >= _count)
            {
                feature = default;
                return false;
            }

            feature = _index == 0 ? _first : _second;
            _index++;
            return true;
        }
    }

    private sealed class PiecewiseObjectiveDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<int, int>
    {
        public bool TryEvaluateCandidateObjective(
            in int solution,
            double currentObjective,
            in int move,
            out double candidateObjective)
        {
            candidateObjective =
                PiecewiseMinProblem.Value(solution + move);
            return true;
        }
    }

    private sealed class SingleFeaturePenaltyDeltaEvaluator :
        IGuidedLocalSearchPenaltyDeltaEvaluator<
            int,
            int,
            int>
    {
        public bool TryEvaluateCandidatePenaltySum(
            in int solution,
            long currentPenaltySum,
            in int move,
            IReadOnlyDictionary<int, int> penalties,
            out long candidatePenaltySum)
        {
            int candidate = solution + move;
            int feature = FeatureFor(candidate);
            penalties.TryGetValue(feature, out int penalty);
            candidatePenaltySum = penalty;
            return true;
        }
    }

    private sealed class PiecewiseMinProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Value(solution);

        public static double Value(int solution) =>
            solution switch
            {
                0 => 5.0,
                1 => 6.0,
                2 => 0.0,
                _ => 1000.0
            };
    }

    private sealed class CountingPiecewiseMinProblem :
        IOptimizationProblem<int>
    {
        public int FullEvaluations { get; private set; }

        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(int solution)
        {
            FullEvaluations++;
            return PiecewiseMinProblem.Value(solution);
        }
    }

    private sealed class PiecewiseMaxProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Maximize;

        public double Evaluate(int solution) =>
            solution switch
            {
                0 => 5.0,
                1 => 4.0,
                2 => 10.0,
                _ => -1000.0
            };
    }

    private static int FeatureFor(int solution) =>
        solution == 0 ? 0 : 1;
}
