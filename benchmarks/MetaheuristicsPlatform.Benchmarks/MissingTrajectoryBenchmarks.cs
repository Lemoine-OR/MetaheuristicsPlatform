using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Acceptance;
using MetaheuristicsPlatform.Algorithms.SA;
using MetaheuristicsPlatform.Algorithms.TA;
using MetaheuristicsPlatform.Algorithms.TS;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Benchmarks;

/// <summary>
/// Compares each optimizer against its own reference on a seeded integer landscape
/// with a bounded evaluation budget and the same exact candidate objective for all methods.
/// </summary>
[MemoryDiagnoser]
public class MissingTrajectoryBenchmarks
{
    private const int DomainSize = 4096;
    private const int MoveCount = 16;
    private const int InitialSolution = 3500;

    private readonly RuggedRingProblem _problem = new();
    private readonly ImmutableSolutionCloner<int> _cloner = new();
    private readonly OptimizationOptions _options = new() { Seed = 123456UL };
    private readonly RingNeighborhood _neighborhood = new();
    private readonly RingMoves _moves = new();
    private readonly RingTabuAttributes _attributes = new();
    private readonly SimulatedAnnealingParameters _annealing = new()
    {
        InitialTemperature = 100,
        StopAtMinimumTemperature = false
    };
    private readonly ThresholdAcceptingParameters _threshold = new()
    {
        InitialThreshold = 80,
        StopAtMinimumThreshold = false
    };
    private readonly GreatDelugeParameters _deluge = new();
    private readonly RecordToRecordTravelParameters _record = new() { Deviation = 80 };
    private readonly LateAcceptanceParameters _late = new() { HistoryLength = 32 };
    private readonly DemonAcceptanceParameters _demon = new() { InitialCredit = 80 };
    private readonly TabuSearchParameters _tabu = new();
    private readonly ReactiveTabuSearchParameters _reactive = new() { MaximumTabuTenure = 12 };
    private MaxEvaluationsStoppingCriterion _stopping = null!;

    [Params(1024, 8192)]
    public int EvaluationBudget { get; set; }

    [GlobalSetup]
    public void Setup() => _stopping = new MaxEvaluationsStoppingCriterion(EvaluationBudget);

    [Benchmark(Description = "simulated-annealing-metropolis")]
    public OptimizationResult<int> SimulatedAnnealing() =>
        Run(new SimulatedAnnealingOptimizer<int, int, int>(
            new DelegateSimulatedAnnealingInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _moves), _annealing);

    [Benchmark(Description = "threshold-accepting-dueck-scheuer-1990")]
    public OptimizationResult<int> ThresholdAccepting() =>
        Run(new ThresholdAcceptingOptimizer<int, int, int>(
            new DelegateThresholdAcceptingInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _moves), _threshold);

    [Benchmark(Description = "great-deluge-dueck-1993")]
    public OptimizationResult<int> GreatDeluge() =>
        Run(new GreatDelugeOptimizer<int, int, int>(
            new DelegateAcceptanceTrajectoryInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _moves), _deluge);

    [Benchmark(Description = "record-to-record-travel-dueck-1993")]
    public OptimizationResult<int> RecordToRecordTravel() =>
        Run(new RecordToRecordTravelOptimizer<int, int, int>(
            new DelegateAcceptanceTrajectoryInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _moves), _record);

    [Benchmark(Description = "late-acceptance-hill-climbing-burke-bykov-2017")]
    public OptimizationResult<int> LateAcceptanceHillClimbing() =>
        Run(new LateAcceptanceHillClimbingOptimizer<int, int, int>(
            new DelegateAcceptanceTrajectoryInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _moves), _late);

    [Benchmark(Description = "demon-based-acceptance-talbi-2009")]
    public OptimizationResult<int> DemonBasedAcceptance() =>
        Run(new DemonBasedAcceptanceOptimizer<int, int, int>(
            new DelegateAcceptanceTrajectoryInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _moves), _demon);

    [Benchmark(Description = "tabu-search-glover")]
    public OptimizationResult<int> TabuSearch() =>
        Run(new TabuSearchOptimizer<int, int, int, int, RingEnumerator>(
            new DelegateTabuSearchInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _attributes, _moves), _tabu);

    [Benchmark(Description = "reactive-tabu-search-battiti-tecchiolli-1994")]
    public OptimizationResult<int> ReactiveTabuSearch() =>
        Run(new ReactiveTabuSearchOptimizer<int, int, int, int, RingEnumerator>(
            new DelegateTabuSearchInitialSolutionGenerator<int>(Initial),
            _neighborhood, _moves, _attributes,
            new DelegateTabuSearchSolutionSignatureProvider<int>(static (in int position) => (ulong)position),
            _moves), _reactive);

    private OptimizationResult<int> Run<TParameters>(
        IMetaheuristic<int, TParameters> optimizer,
        TParameters parameters)
        where TParameters : IMetaheuristicParameters
    {
        return optimizer.Optimize(_problem, parameters, _cloner, _stopping, _options);
    }

    private static int Initial(IOptimizationProblem<int> problem, IRandomSource random) => InitialSolution;

    private static int Candidate(int solution, int move) => (solution + move) & (DomainSize - 1);

    private static int Step(int index) => (index & 1) == 0 ? (index / 2) + 1 : -((index / 2) + 1);

    private sealed class RuggedRingProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) => Objective(solution);

        public static double Objective(int solution)
        {
            // The ring wraps both moves and distance. The bounded ripple creates local minima near the target.
            int distance = Math.Abs(solution - 777);
            distance = Math.Min(distance, DomainSize - distance);
            int ripple = ((1031 * solution) % 257) * 32;
            return (distance * distance) + ripple;
        }
    }

    private sealed class RingNeighborhood :
        IStochasticNeighborhood<int, int>,
        IEnumeratedNeighborhood<int, int, RingEnumerator>
    {
        public bool TrySampleMove(in int solution, IRandomSource random, out int move)
        {
            move = Step(random.NextInt32(MoveCount));
            return true;
        }

        public RingEnumerator GetEnumerator(in int solution) => new();
    }

    private struct RingEnumerator : INeighborhoodEnumerator<int>
    {
        private int _index;

        public bool MoveNext(out int move)
        {
            if (_index == MoveCount)
            {
                move = default;
                return false;
            }

            move = Step(_index++);
            return true;
        }
    }

    private sealed class RingMoves : IReversibleMoveOperator<int, int, int>, IMoveObjectiveDeltaEvaluator<int, int>
    {
        public int CaptureUndo(in int solution, in int move) => solution;

        public void Apply(ref int solution, in int move) => solution = Candidate(solution, move);

        public void Undo(ref int solution, in int move, in int undo) => solution = undo;

        public bool TryEvaluateCandidateObjective(in int solution, double currentObjective, in int move,
            out double candidateObjective)
        {
            candidateObjective = RuggedRingProblem.Objective(Candidate(solution, move));
            return true;
        }
    }

    private sealed class RingTabuAttributes : ITabuAttributeProvider<int, int, int>
    {
        public int GetCandidateAttribute(in int solution, in int move) => Candidate(solution, move);

        public int GetAttributeToForbid(in int solution, in int move) => solution;
    }
}
