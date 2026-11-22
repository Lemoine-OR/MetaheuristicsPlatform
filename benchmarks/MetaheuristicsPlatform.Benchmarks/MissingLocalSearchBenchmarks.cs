using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Benchmarks;

/// <summary>
/// Composed local search on a bounded landscape with multiple basins.
/// End-to-end cases include the optimizer, stopping and ownership used by production runs.
/// </summary>
[MemoryDiagnoser]
public class MissingLocalSearchBenchmarks
{
    private const int BasinWidth = 32;
    private static readonly ImmutableSolutionCloner<int> Cloner = new();
    private static readonly OptimizationOptions Options = new() { Seed = 123456UL };
    private static readonly MaxEvaluationsStoppingCriterion Budget = new(4_096);
    private static readonly StepOperator MoveOperator = new();
    private static readonly StepNeighborhood FineSteps = new(1);

    private MultiBasinProblem _problem = null!;
    private BoundedMoves _bounds = null!;
    private INeighborhoodSearchInitialSolutionGenerator<int> _starts = null!;
    private ILocalSearchProcedure<int> _descent = null!;
    private ILocalSearchProcedure<int>[] _localNeighborhoods = null!;
    private ISolutionPerturbation<int>[] _shakingNeighborhoods = null!;

    [Params(256, 4096)]
    public int Capacity { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem = new MultiBasinProblem(Capacity * 3 / 4);
        _bounds = new BoundedMoves(Capacity);
        _starts = new DelegateNeighborhoodSearchInitialSolutionGenerator<int>(
            (_, random) => random.NextInt32(Capacity));
        _descent = new MoveLocalSearchProcedure<int, int, int, StepEnumerator>(
            FineSteps, MoveOperator, LocalSearchSelectionPolicy.BestImprovement,
            moveApplicability: _bounds);
        _localNeighborhoods =
        [
            _descent,
            new MoveLocalSearchProcedure<int, int, int, StepEnumerator>(
                new StepNeighborhood(BasinWidth), MoveOperator,
                LocalSearchSelectionPolicy.BestImprovement, moveApplicability: _bounds)
        ];
        _shakingNeighborhoods =
        [
            new BoundedShake(BasinWidth, Capacity),
            new BoundedShake(BasinWidth * 4, Capacity)
        ];
    }

    [Benchmark(Description = "local-search-best-improvement")]
    public OptimizationResult<int> BestImprovement() =>
        new BestImprovementLocalSearchOptimizer<int, int, int, StepEnumerator>(
            _starts, FineSteps, MoveOperator, moveApplicability: _bounds)
            .Optimize(_problem, new LocalSearchParameters(), Cloner, Budget, Options);

    [Benchmark(Description = "local-search-first-improvement")]
    public OptimizationResult<int> FirstImprovement() =>
        new FirstImprovementLocalSearchOptimizer<int, int, int, StepEnumerator>(
            _starts, FineSteps, MoveOperator, moveApplicability: _bounds)
            .Optimize(_problem, new LocalSearchParameters(), Cloner, Budget, Options);

    [Benchmark(Description = "multi-start-local-search")]
    public OptimizationResult<int> MultiStart() =>
        new MultiStartLocalSearchOptimizer<int>(_starts, _descent)
            .Optimize(_problem, new MultiStartLocalSearchParameters { MaximumStarts = 32 },
                Cloner, Budget, Options);

    [Benchmark(Description = "iterated-local-search-lourenco-martin-stutzle")]
    public OptimizationResult<int> IteratedLocalSearch() =>
        new IteratedLocalSearchOptimizer<int>(_starts, _descent, _shakingNeighborhoods[1])
            .Optimize(_problem, new IteratedLocalSearchParameters { MaximumIterations = 64 },
                Cloner, Budget, Options);

    [Benchmark(Description = "variable-neighborhood-descent")]
    public OptimizationResult<int> VariableNeighborhoodDescent() =>
        new VariableNeighborhoodDescentOptimizer<int>(_starts, _localNeighborhoods)
            .Optimize(_problem, new VariableNeighborhoodDescentParameters(), Cloner, Budget, Options);

    [Benchmark(Description = "variable-neighborhood-search-mladenovic-hansen")]
    public OptimizationResult<int> VariableNeighborhoodSearch() =>
        new VariableNeighborhoodSearchOptimizer<int>(_starts, _shakingNeighborhoods, _descent)
            .Optimize(_problem, new VariableNeighborhoodSearchParameters { MaximumCycles = 32 },
                Cloner, Budget, Options);

    [Benchmark(Description = "guided-local-search-voudouris-tsang-1999")]
    public OptimizationResult<int> GuidedLocalSearch() =>
        new GuidedLocalSearchOptimizer<int, int, int, StepEnumerator, int, FeatureEnumerator>(
            _starts, FineSteps, MoveOperator, new LocationFeatureModel(_problem),
            moveApplicability: _bounds)
            .Optimize(_problem, new GuidedLocalSearchParameters
            {
                MaximumPenaltyUpdates = 24,
                PenaltyWeight = 2.0
            }, Cloner, Budget, Options);

    [Benchmark(Description = "reduced-variable-neighborhood-search")]
    public OptimizationResult<int> ReducedVariableNeighborhoodSearch() =>
        new ReducedVariableNeighborhoodSearchOptimizer<int>(_starts, _shakingNeighborhoods)
            .Optimize(_problem, new ReducedVariableNeighborhoodSearchParameters { MaximumCycles = 32 },
                Cloner, Budget, Options);

    [Benchmark(Description = "general-variable-neighborhood-search")]
    public OptimizationResult<int> GeneralVariableNeighborhoodSearch() =>
        new GeneralVariableNeighborhoodSearchOptimizer<int>(
            _starts, _shakingNeighborhoods, _localNeighborhoods)
            .Optimize(_problem, new GeneralVariableNeighborhoodSearchParameters { MaximumCycles = 32 },
                Cloner, Budget, Options);

    [Benchmark(Description = "skewed-variable-neighborhood-search-hansen-mladenovic-2001")]
    public OptimizationResult<int> SkewedVariableNeighborhoodSearch() =>
        new SkewedVariableNeighborhoodSearchOptimizer<int>(
            _starts, _shakingNeighborhoods, _descent, new AbsoluteDistance())
            .Optimize(_problem, new SkewedVariableNeighborhoodSearchParameters
            {
                MaximumCycles = 32,
                Alpha = 1.5
            }, Cloner, Budget, Options);

    private sealed class MultiBasinProblem(int target) : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution)
        {
            // The periodic term creates local optima; the distance term orders the basins.
            int phase = solution % BasinWidth;
            int distanceToBasin = Math.Min(phase, BasinWidth - phase);
            return 16 * distanceToBasin * distanceToBasin + Math.Abs(solution - target);
        }
    }

    private sealed class StepNeighborhood(int step) : IEnumeratedNeighborhood<int, int, StepEnumerator>
    {
        public StepEnumerator GetEnumerator(in int solution) => new(step);
    }

    private struct StepEnumerator(int step) : INeighborhoodEnumerator<int>
    {
        private int _index;

        public bool MoveNext(out int move)
        {
            move = _index == 0 ? -step : step;
            if (_index < 2)
            {
                _index++;
                return true;
            }

            return false;
        }
    }

    private sealed class StepOperator : IReversibleMoveOperator<int, int, int>
    {
        public int CaptureUndo(in int solution, in int move) => solution;
        public void Apply(ref int solution, in int move) => solution += move;
        public void Undo(ref int solution, in int move, in int undo) => solution = undo;
    }

    private sealed class BoundedMoves(int capacity) : IMoveApplicability<int, int>
    {
        public bool IsApplicable(in int solution, in int move) => (uint)(solution + move) < capacity;
    }

    private sealed class BoundedShake(int radius, int capacity) : ISolutionPerturbation<int>
    {
        public void Perturb(ref int solution, IOptimizationProblem<int> problem, IRandomSource random) =>
            solution = Math.Clamp(solution + random.NextInt32(-radius, radius + 1), 0, capacity - 1);
    }

    private sealed class AbsoluteDistance : ISolutionDistance<int>
    {
        public double Distance(in int first, in int second) => Math.Abs(first - second);
    }

    private sealed class LocationFeatureModel(MultiBasinProblem problem) :
        IGuidedLocalSearchFeatureModel<int, int, FeatureEnumerator>
    {
        public FeatureEnumerator GetEnumerator(in int solution) => new(solution);

        public double GetFeatureCost(in int solution, in int feature) => 1.0 + problem.Evaluate(feature);
    }

    private struct FeatureEnumerator(int location) : IGuidedLocalSearchFeatureEnumerator<int>
    {
        private bool _emitted;

        public bool MoveNext(out int feature)
        {
            feature = location;
            if (_emitted)
            {
                return false;
            }

            _emitted = true;
            return true;
        }
    }
}
