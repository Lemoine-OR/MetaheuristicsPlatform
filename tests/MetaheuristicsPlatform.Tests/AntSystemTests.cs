using MetaheuristicsPlatform.Algorithms.AntColony;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class AntSystemTests
{
    [Fact]
    public void DescriptorUsesStableCanonicalIdAndReference()
    {
        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> optimizer =
            CreateOptimizer();

        Assert.Equal(
            "ant-system-dorigo-maniezzo-colorni-1996",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1109/3477.484436");
    }

    [Fact]
    public void DefaultParametersAreValid()
    {
        new AntSystemParameters().Validate();
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { AntCount = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { MaximumIterations = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { Alpha = -1.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { Beta = -1.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { EvaporationRate = 0.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { EvaporationRate = 1.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { InitialPheromone = 0.0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AntSystemParameters { MaximumConstructionSteps = 0 }.Validate());
    }

    [Fact]
    public void FullIterationsUseExactlyOneObjectiveEvaluationPerAnt()
    {
        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> optimizer =
            CreateOptimizer();

        OptimizationResult<int> result =
            optimizer.Optimize(
                new TargetProblem(),
                new AntSystemParameters
                {
                    AntCount = 4,
                    MaximumIterations = 3,
                    Alpha = 1.0,
                    Beta = 1.0
                },
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 42UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(12, result.Statistics.Evaluations);
        Assert.Equal(3, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumAntSystemIterations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void GlobalEvaluationBudgetStopsInsideColonyWithoutOvershoot()
    {
        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> optimizer =
            CreateOptimizer();

        OptimizationResult<int> result =
            optimizer.Optimize(
                new TargetProblem(),
                new AntSystemParameters
                {
                    AntCount = 10,
                    MaximumIterations = 10
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(5),
                new OptimizationOptions { Seed = 7UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Statistics.Evaluations);
        Assert.Equal("MaxEvaluations", result.StopDecision.Criterion);
        Assert.Equal(0, result.Statistics.Iterations);
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> first =
            CreateOptimizer();

        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> second =
            CreateOptimizer();

        AntSystemParameters parameters =
            new()
            {
                AntCount = 8,
                MaximumIterations = 5,
                Alpha = 1.0,
                Beta = 2.0
            };

        OptimizationResult<int> a =
            first.Optimize(
                new TargetProblem(),
                parameters,
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 123456UL },
                cancellationToken: TestContext.Current.CancellationToken);

        OptimizationResult<int> b =
            second.Optimize(
                new TargetProblem(),
                parameters,
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 123456UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(a.BestSolution, b.BestSolution);
        Assert.Equal(a.BestFitness, b.BestFitness);
        Assert.Equal(a.Statistics.Evaluations, b.Statistics.Evaluations);
    }

    [Fact]
    public void PositiveInverseDepositRejectsUnsupportedObjectiveScales()
    {
        var policy =
            new PositiveInverseObjectiveAntSystemDepositPolicy<int>();

        Assert.Throws<InvalidOperationException>(
            () => policy.GetDeposit(
                1,
                1.0,
                0,
                1,
                new MaximizeProblem()));

        Assert.Throws<InvalidOperationException>(
            () => policy.GetDeposit(
                1,
                0.0,
                0,
                1,
                new TargetProblem()));
    }

    [Fact]
    public void IncompleteConstructionWithoutCandidatesIsRejected()
    {
        var optimizer =
            new AntSystemOptimizer<int, int, int, EmptyCandidateEnumerator>(
                new EmptyModel(),
                new ConstantAntSystemDepositPolicy<int>());

        Assert.Throws<InvalidOperationException>(
            () => optimizer.Optimize(
                new TargetProblem(),
                new AntSystemParameters
                {
                    AntCount = 1,
                    MaximumIterations = 1
                },
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 1UL },
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void NonPositiveHeuristicIsRejectedWhenBetaIsPositive()
    {
        var optimizer =
            new AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator>(
                new ZeroHeuristicModel(),
                new ConstantAntSystemDepositPolicy<int>());

        Assert.Throws<InvalidOperationException>(
            () => optimizer.Optimize(
                new TargetProblem(),
                new AntSystemParameters
                {
                    AntCount = 1,
                    MaximumIterations = 1,
                    Beta = 1.0
                },
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 2UL },
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void StableIdSupportsTypedFactoryRegistration()
    {
        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> configured =
            CreateOptimizer();

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.AntSystem,
            () => configured,
            replace: true);

        AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator> created =
            MetaheuristicFactory.Create<
                AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator>>(
                MetaheuristicAlgorithmIds.AntSystem);

        Assert.Same(configured, created);
    }

    private static AntSystemOptimizer<int, int, int, ThreeCandidateEnumerator>
        CreateOptimizer() =>
        new(
            new SumToTargetConstructionModel(),
            new ConstantAntSystemDepositPolicy<int>(1.0));

    private sealed class TargetProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            Math.Abs(10 - solution) + 1.0;
    }

    private sealed class MaximizeProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Maximize;

        public double Evaluate(int solution) => solution;
    }

    private struct ThreeCandidateEnumerator :
        IAntColonyCandidateEnumerator<int>
    {
        private int _index;

        public bool MoveNext(out int component)
        {
            _index++;

            if (_index <= 3)
            {
                component = _index;
                return true;
            }

            component = default;
            return false;
        }
    }

    private readonly struct EmptyCandidateEnumerator :
        IAntColonyCandidateEnumerator<int>
    {
        public bool MoveNext(out int component)
        {
            component = default;
            return false;
        }
    }

    private sealed class SumToTargetConstructionModel :
        IAntColonyConstructionModel<int, int, int, ThreeCandidateEnumerator>
    {
        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            solution >= 10;

        public ThreeCandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public int GetPheromoneKey(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            (solution * 10) + component;

        public double EvaluateHeuristic(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            1.0 / (1.0 + Math.Abs(10 - (solution + component)));

        public void ApplyComponent(
            ref int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            solution += component;
    }

    private sealed class ZeroHeuristicModel :
        IAntColonyConstructionModel<int, int, int, ThreeCandidateEnumerator>
    {
        public int CreateInitialSolution(
            IOptimizationProblem<int> problem,
            IRandomSource random) => 0;

        public bool IsComplete(
            in int solution,
            IOptimizationProblem<int> problem) =>
            solution > 0;

        public ThreeCandidateEnumerator GetCandidateEnumerator(
            in int solution,
            IOptimizationProblem<int> problem) =>
            new();

        public int GetPheromoneKey(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            component;

        public double EvaluateHeuristic(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            0.0;

        public void ApplyComponent(
            ref int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            solution = component;
    }

    private sealed class EmptyModel :
        IAntColonyConstructionModel<int, int, int, EmptyCandidateEnumerator>
    {
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

        public int GetPheromoneKey(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            component;

        public double EvaluateHeuristic(
            in int solution,
            in int component,
            IOptimizationProblem<int> problem) =>
            1.0;

        public void ApplyComponent(
            ref int solution,
            in int component,
            IOptimizationProblem<int> problem)
        {
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
}
