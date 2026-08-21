using MetaheuristicsPlatform.Algorithms.IteratedGreedy;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedIteratedGreedyTests
{
    [Fact]
    public void StagnationEscalatingDestructionSizeIncreasesAndCaps()
    {
        var policy =
            new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
                minimumDestructionSize: 2,
                maximumDestructionSize: 5,
                stagnationWindow: 3,
                increment: 2);

        Assert.Equal(
            2,
            Select(policy, 0));

        Assert.Equal(
            2,
            Select(policy, 2));

        Assert.Equal(
            4,
            Select(policy, 3));

        Assert.Equal(
            5,
            Select(policy, 6));

        Assert.Equal(
            5,
            Select(policy, 100));
    }

    [Fact]
    public void LegacyConstructorKeepsFixedParameterDestructionSize()
    {
        var observed = new List<int>();

        var algorithm =
            new IteratedGreedyOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestruction(observed),
                new WorseningConstruction(),
                ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance);

        algorithm.Optimize(
            new MinProblem(),
            new IteratedGreedyParameters
            {
                DestructionSize = 3,
                MaximumIterations = 2
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            new[] { 3, 3 },
            observed);
    }

    [Fact]
    public void AdaptiveDestructionUsesBestSoFarStagnation()
    {
        var observed = new List<int>();

        var policy =
            new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
                minimumDestructionSize: 1,
                maximumDestructionSize: 3,
                stagnationWindow: 1);

        var algorithm =
            new IteratedGreedyOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestruction(observed),
                new WorseningConstruction(),
                ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance,
                policy);

        algorithm.Optimize(
            new MinProblem(),
            new IteratedGreedyParameters
            {
                DestructionSize = 1,
                MaximumIterations = 3
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            new[] { 1, 2, 3 },
            observed);
    }

    [Fact]
    public void PartialImprovementRunsStrictlyBetweenDestroyAndReconstruct()
    {
        var trace = new List<string>();

        var algorithm =
            new IteratedGreedyOptimizer<int,int>(
                new ConstantInitial(10),
                new TraceDestruction(trace),
                new TraceConstruction(trace),
                ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance,
                FixedIteratedGreedyDestructionSizePolicy.Instance,
                new TracePartialImprovement(trace));

        algorithm.Optimize(
            new MinProblem(),
            new IteratedGreedyParameters
            {
                DestructionSize = 2,
                MaximumIterations = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            new[]
            {
                "destroy",
                "partial-improve",
                "reconstruct"
            },
            trace);
    }

    [Fact]
    public void StoppingAfterCandidateEvaluationSeesFiniteLastCandidateObjective()
    {
        var algorithm =
            new IteratedGreedyOptimizer<int,int>(
                new ConstantInitial(10),
                new RecordingDestruction(new List<int>()),
                new ImprovingConstruction(),
                ImprovingOnlyIteratedGreedyAcceptancePolicy.Instance);

        var result =
            algorithm.Optimize(
                new MinProblem(),
                new IteratedGreedyParameters
                {
                    DestructionSize = 1,
                    MaximumIterations = 5
                },
                new ImmutableSolutionCloner<int>(),
                new CandidateObjectiveAwareStoppingCriterion(),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "CandidateObjectiveObserved",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void ReviewedReferenceAuthorsAreExact()
    {
        Assert.Equal(
            "Kuo-Ching Ying; Shih-Wei Lin; Chen-Yang Cheng; Cheng-Ding He",
            IteratedGreedyReferences.IteratedReferenceGreedy2017.Authors);

        Assert.Equal(
            "Xue-Lei Jing; Quan-Ke Pan; Liang Gao; Yu-Long Wang",
            IteratedGreedyReferences.JingPanGaoWang2020.Authors);

        Assert.Equal(
            "Yuan-Zhen Li; Quan-Ke Pan; Jun-Qing Li; Liang Gao; Mehmet Fatih Tasgetiren",
            IteratedGreedyReferences.LiPanLiGaoTasgetiren2021.Authors);

        Assert.Equal(
            "10.1016/j.eswa.2025.130422",
            IteratedGreedyReferences.ZhangQianHuLiYang2026.Doi);
    }

    [Fact]
    public void InvalidAdaptiveDestructionParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
                0, 3, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
                4, 3, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
                1, 3, 0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
                1, 3, 1, 0));
    }

    private static int Select(
        IIteratedGreedyDestructionSizePolicy policy,
        int stagnation)
    {
        var context =
            new IteratedGreedyDestructionSizeContext(
                OptimizationSense.Minimize,
                Iteration: 1,
                BaseDestructionSize: 4,
                ConsecutiveNonImprovingIterations: stagnation,
                CurrentObjective: 10.0,
                BestObjective: 9.0);

        return
            policy.SelectDestructionSize(
                in context);
    }

    private sealed class MinProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution) =>
            solution;
    }

    private sealed class ConstantInitial :
        MetaheuristicsPlatform.Algorithms.Neighborhood.INeighborhoodSearchInitialSolutionGenerator<int>
    {
        private readonly int _value;

        public ConstantInitial(
            int value) =>
            _value = value;

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            _value;
    }

    private sealed class RecordingDestruction :
        IIteratedGreedyDestruction<int,int>
    {
        private readonly List<int> _sizes;

        public RecordingDestruction(
            List<int> sizes) =>
            _sizes = sizes;

        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _sizes.Add(
                destructionSize);

            partialSolution -=
                destructionSize;

            return
                destructionSize;
        }
    }

    private sealed class WorseningConstruction :
        IIteratedGreedyConstruction<int,int>
    {
        public void Reconstruct(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution +=
                removedComponents + 2;
        }
    }

    private sealed class ImprovingConstruction :
        IIteratedGreedyConstruction<int,int>
    {
        public void Reconstruct(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            partialSolution +=
                removedComponents - 1;
        }
    }

    private sealed class TraceDestruction :
        IIteratedGreedyDestruction<int,int>
    {
        private readonly List<string> _trace;

        public TraceDestruction(
            List<string> trace) =>
            _trace = trace;

        public int Destroy(
            ref int partialSolution,
            int destructionSize,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add("destroy");
            partialSolution -= destructionSize;
            return destructionSize;
        }
    }

    private sealed class TracePartialImprovement :
        IIteratedGreedyPartialSolutionImprovement<int,int>
    {
        private readonly List<string> _trace;

        public TracePartialImprovement(
            List<string> trace) =>
            _trace = trace;

        public void Improve(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _trace.Add("partial-improve");
            partialSolution--;
        }
    }

    private sealed class TraceConstruction :
        IIteratedGreedyConstruction<int,int>
    {
        private readonly List<string> _trace;

        public TraceConstruction(
            List<string> trace) =>
            _trace = trace;

        public void Reconstruct(
            ref int partialSolution,
            in int removedComponents,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            _trace.Add("reconstruct");
            partialSolution += removedComponents;
        }
    }

    private sealed class CandidateObjectiveAwareStoppingCriterion :
        IStoppingCriterion
    {
        public string Name =>
            "CandidateObjectiveObserved";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense)
        {
            if (state.Evaluations < 2)
                return StoppingDecision.Continue(Name);

            if (state.AlgorithmState is not IteratedGreedyState igState ||
                !double.IsFinite(igState.LastCandidateObjective))
            {
                throw new InvalidOperationException(
                    "Stopping criterion received stale Iterated Greedy candidate state.");
            }

            return StoppingDecision.Stop(Name);
        }
    }
}
