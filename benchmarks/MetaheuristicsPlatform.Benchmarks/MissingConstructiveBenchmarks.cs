using System.Numerics;
using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Constructive;
using MetaheuristicsPlatform.Algorithms.IteratedGreedy;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Algorithms.ScatterSearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Benchmarks;

/// <summary>End-to-end constructive and population searches on a fixed weighted subset problem.</summary>
[MemoryDiagnoser]
public class MissingConstructiveBenchmarks
{
    private const int ItemCount = 16;
    private const int SelectionSize = 8;
    private const int ItemMask = (1 << ItemCount) - 1;
    private static readonly SubsetProblem Problem = new();
    private static readonly ImmutableSolutionCloner<int> Cloner = new();
    private static readonly OptimizationOptions Options = new() { Seed = 123456UL };
    private static readonly MaxEvaluationsStoppingCriterion Stop = new(8_192);
    private static readonly CanonicalGraspConstructionProcedure<int, int, BitEnumerator> Construction =
        new(new SubsetConstruction());
    private static readonly ExchangeNeighborhood Neighborhood = new();
    private static readonly ExchangeOperator MoveOperator = new();
    private static readonly MoveLocalSearchProcedure<int, int, int, ExchangeEnumerator> LocalSearch =
        new(Neighborhood, MoveOperator, LocalSearchSelectionPolicy.BestImprovement, maximumAcceptedMoves: 2);

    private readonly GraspOptimizer<int> _grasp = new(Construction, LocalSearch);
    private readonly ReactiveGraspOptimizer<int> _reactive = new(Construction, LocalSearch);
    private readonly GraspPathRelinkingOptimizer<int> _relinking = new(
        Construction,
        LocalSearch,
        new GreedyForwardPathRelinkingProcedure<int, int, int, ExchangeEnumerator>(
            Neighborhood, Neighborhood, MoveOperator),
        Neighborhood);
    private readonly ScatterSearchOptimizer<int> _scatter = new(
        new DelegateScatterSearchDiversificationGenerationMethod<int>(static (_, random) => RandomSubset(random)),
        new DelegateScatterSearchSolutionCombinationMethod<int>(static (subset, _, random) =>
            [GreedyComplete(subset.Members[0].Solution & subset.Members[1].Solution),
                CompleteFromMask(subset.Members[0].Solution | subset.Members[1].Solution, random)]),
        new DelegateScatterSearchDistance<int>(static (in int left, in int right) => Hamming(left, right)));
    private readonly IteratedGreedyOptimizer<int, int> _iterated = new(
        new DelegateNeighborhoodSearchInitialSolutionGenerator<int>(static (_, random) => RandomSubset(random)),
        new DelegateIteratedGreedyDestruction<int, int>(DestroySubset),
        new DelegateIteratedGreedyConstruction<int, int>(ReconstructSubset),
        new ConstantTemperatureIteratedGreedyAcceptancePolicy(2.0),
        LocalSearch);

    private readonly GraspParameters _graspParameters = new()
    {
        MaximumIterations = 12,
        MaximumConstructionSteps = SelectionSize,
        Alpha = 0.55
    };

    private readonly ReactiveGraspParameters _reactiveParameters = new()
    {
        MaximumIterations = 32,
        MaximumConstructionSteps = SelectionSize,
        AlphaValues = [0.2, 0.6, 1.0],
        ProbabilityUpdatePeriod = 4
    };

    private readonly GraspPathRelinkingParameters _relinkingParameters = new()
    {
        MaximumIterations = 12,
        MaximumConstructionSteps = SelectionSize,
        Alpha = 0.7,
        ElitePoolSize = 5,
        MinimumEliteDistance = 2,
        MaximumPathSteps = SelectionSize
    };

    private readonly ScatterSearchParameters _scatterParameters = new()
    {
        DiversificationPopulationSize = 24,
        ReferenceSetSize = 8,
        QualityReferenceSetSize = 4,
        MaximumIterations = 12
    };

    private readonly IteratedGreedyParameters _iteratedParameters = new()
    {
        DestructionSize = 3,
        MaximumIterations = 16
    };

    [Benchmark(Description = "grasp-feo-resende-1995")]
    public OptimizationResult<int> Grasp() =>
        _grasp.Optimize(Problem, _graspParameters, Cloner, Stop, Options);

    [Benchmark(Description = "reactive-grasp-prais-ribeiro-2000")]
    public OptimizationResult<int> ReactiveGrasp() =>
        _reactive.Optimize(Problem, _reactiveParameters, Cloner, Stop, Options);

    [Benchmark(Description = "grasp-path-relinking")]
    public OptimizationResult<int> GraspPathRelinking() =>
        _relinking.Optimize(Problem, _relinkingParameters, Cloner, Stop, Options);

    [Benchmark(Description = "scatter-search-marti-laguna-glover-2006")]
    public OptimizationResult<int> ScatterSearch() =>
        _scatter.Optimize(Problem, _scatterParameters, Cloner, Stop, Options);

    [Benchmark(Description = "iterated-greedy-ruiz-stutzle-2007")]
    public OptimizationResult<int> IteratedGreedy() =>
        _iterated.Optimize(Problem, _iteratedParameters, Cloner, Stop, Options);

    private static int Hamming(int left, int right) => BitOperations.PopCount((uint)(left ^ right));

    private static int RandomSubset(IRandomSource random) => CompleteFromMask(ItemMask, random);

    private static int CompleteFromMask(int pool, IRandomSource random)
    {
        int subset = 0;
        while (BitOperations.PopCount((uint)subset) < SelectionSize)
        {
            int bit = PickBit(pool, random);
            subset |= bit;
            pool ^= bit;
        }

        return subset;
    }

    private static int PickBit(int mask, IRandomSource random)
    {
        int index = random.NextInt32(BitOperations.PopCount((uint)mask));
        for (int i = 0; i < index; i++)
        {
            mask &= mask - 1;
        }

        return mask & -mask;
    }

    private static int GreedyComplete(int partial)
    {
        while (BitOperations.PopCount((uint)partial) < SelectionSize)
        {
            var candidates = new BitEnumerator(ItemMask ^ partial);
            int bestBit = 0;
            int bestCost = int.MaxValue;
            while (candidates.MoveNext(out int bit))
            {
                int cost = SubsetProblem.Cost(partial | bit);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestBit = bit;
                }
            }

            partial |= bestBit;
        }

        return partial;
    }

    private static int DestroySubset(
        ref int subset, int destructionSize, IOptimizationProblem<int> problem, IRandomSource random)
    {
        for (int i = 0; i < destructionSize; i++)
        {
            subset ^= PickBit(subset, random);
        }

        return destructionSize;
    }

    private static void ReconstructSubset(
        ref int subset, in int removedCount, IOptimizationProblem<int> problem, IRandomSource random) =>
        subset = GreedyComplete(subset);

    private sealed class SubsetProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int subset)
        {
            if (BitOperations.PopCount((uint)subset) != SelectionSize)
            {
                throw new ArgumentException("Only complete subsets can be evaluated.", nameof(subset));
            }

            return Cost(subset);
        }

        public static int Cost(int subset)
        {
            // Adjacent choices interact, so the greedy scores depend on the partial subset.
            int cost = 1 + 7 * BitOperations.PopCount((uint)(subset & (subset >> 1)));
            var items = new BitEnumerator(subset);
            while (items.MoveNext(out int bit))
            {
                int index = BitOperations.TrailingZeroCount((uint)bit);
                cost += 1 + (11 * index) % 19;
            }

            return cost;
        }
    }

    private sealed class SubsetConstruction : IGraspConstructionModel<int, int, BitEnumerator>
    {
        public GraspGreedyScoreSense GreedyScoreSense => GraspGreedyScoreSense.Minimize;

        public int CreateInitialSolution(IOptimizationProblem<int> problem, IRandomSource random) => 0;

        public bool IsComplete(in int solution, IOptimizationProblem<int> problem) =>
            BitOperations.PopCount((uint)solution) == SelectionSize;

        public BitEnumerator GetCandidateEnumerator(in int solution, IOptimizationProblem<int> problem) =>
            new(ItemMask ^ solution);

        public double EvaluateGreedyScore(in int solution, in int candidate, IOptimizationProblem<int> problem) =>
            SubsetProblem.Cost(solution | candidate);

        public void ApplyCandidate(ref int solution, in int candidate, IOptimizationProblem<int> problem) =>
            solution |= candidate;
    }

    private struct BitEnumerator(int available) : IGraspCandidateEnumerator<int>
    {
        private int _available = available;

        public bool MoveNext(out int bit)
        {
            bit = _available & -_available;
            _available ^= bit;
            return bit != 0;
        }
    }

    private struct ExchangeEnumerator(int removals, int additions) : INeighborhoodEnumerator<int>
    {
        private BitEnumerator _removals = new(removals);
        private BitEnumerator _additions;
        private int _removed;

        public bool MoveNext(out int move)
        {
            int added;
            while (!_additions.MoveNext(out added))
            {
                if (!_removals.MoveNext(out _removed))
                {
                    move = 0;
                    return false;
                }

                _additions = new BitEnumerator(additions);
            }

            move = _removed | added;
            return true;
        }
    }

    private sealed class ExchangeNeighborhood :
        IEnumeratedNeighborhood<int, int, ExchangeEnumerator>,
        IPathRelinkingNeighborhood<int, int, ExchangeEnumerator>,
        IPathRelinkingDistance<int>
    {
        public ExchangeEnumerator GetEnumerator(in int solution) =>
            new(solution, ItemMask ^ solution);

        public ExchangeEnumerator GetEnumerator(in int current, in int guiding, IOptimizationProblem<int> problem) =>
            new(current & ~guiding, guiding & ~current);

        public int GetDistance(in int first, in int second, IOptimizationProblem<int> problem) =>
            Hamming(first, second);
    }

    private sealed class ExchangeOperator : IReversibleMoveOperator<int, int, int>
    {
        public int CaptureUndo(in int solution, in int move) => solution;

        public void Apply(ref int solution, in int move) => solution ^= move;

        public void Undo(ref int solution, in int move, in int undo) => solution = undo;
    }
}
