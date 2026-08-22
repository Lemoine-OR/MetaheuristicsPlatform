using MetaheuristicsPlatform.Algorithms.AntColony;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedAntColonyTests
{
    [Fact]
    public void AcsDescriptorUsesCanonicalReference()
    {
        var optimizer = CreateAcs();

        Assert.Equal(
            "ant-colony-system-dorigo-gambardella-1997",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1109/4235.585892");
    }

    [Fact]
    public void MmasDescriptorUsesCanonicalReference()
    {
        var optimizer = CreateMmas();

        Assert.Equal(
            "max-min-ant-system-stutzle-hoos-2000",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi == "10.1016/S0167-739X(00)00043-1");
    }

    [Fact]
    public void DefaultAdvancedParametersAreValid()
    {
        new AntColonySystemParameters().Validate();
        new MaxMinAntSystemParameters().Validate();
    }

    [Fact]
    public void AcsUsesExactlyOneObjectiveEvaluationPerConstructedAnt()
    {
        OptimizationResult<int> result =
            CreateAcs().Optimize(
                new TargetProblem(),
                new AntColonySystemParameters
                {
                    AntCount = 4,
                    MaximumIterations = 3
                },
                new ImmutableSolutionCloner<int>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 42UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(12, result.Statistics.Evaluations);
        Assert.Equal(3, result.Statistics.Iterations);
    }

    [Fact]
    public void MmasBudgetStopsInsideColonyWithoutOvershoot()
    {
        OptimizationResult<int> result =
            CreateMmas().Optimize(
                new TargetProblem(),
                new MaxMinAntSystemParameters
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
    public void SameSeedProducesSameAcsResult()
    {
        var a = CreateAcs().Optimize(
            new TargetProblem(),
            new AntColonySystemParameters
            {
                AntCount = 6,
                MaximumIterations = 4
            },
            new ImmutableSolutionCloner<int>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 1234UL },
            cancellationToken: TestContext.Current.CancellationToken);

        var b = CreateAcs().Optimize(
            new TargetProblem(),
            new AntColonySystemParameters
            {
                AntCount = 6,
                MaximumIterations = 4
            },
            new ImmutableSolutionCloner<int>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 1234UL },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(a.BestSolution, b.BestSolution);
        Assert.Equal(a.BestFitness, b.BestFitness);
        Assert.Equal(a.Statistics.Evaluations, b.Statistics.Evaluations);
    }

    [Fact]
    public void StableIdsSupportTypedFactoryRegistration()
    {
        var acs = CreateAcs();
        var mmas = CreateMmas();

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.AntColonySystem,
            () => acs,
            replace: true);

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.MaxMinAntSystem,
            () => mmas,
            replace: true);

        Assert.Same(
            acs,
            MetaheuristicFactory.Create<
                AntColonySystemOptimizer<int,int,int,ThreeCandidateEnumerator>>(
                MetaheuristicAlgorithmIds.AntColonySystem));

        Assert.Same(
            mmas,
            MetaheuristicFactory.Create<
                MaxMinAntSystemOptimizer<int,int,int,ThreeCandidateEnumerator>>(
                MetaheuristicAlgorithmIds.MaxMinAntSystem));
    }

    private static AntColonySystemOptimizer<
        int,int,int,ThreeCandidateEnumerator> CreateAcs() =>
        new(
            new SumToTargetConstructionModel(),
            new ConstantAntSystemDepositPolicy<int>(1.0));

    private static MaxMinAntSystemOptimizer<
        int,int,int,ThreeCandidateEnumerator> CreateMmas() =>
        new(
            new SumToTargetConstructionModel(),
            new ConstantAntSystemDepositPolicy<int>(0.1));

    private sealed class TargetProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) =>
            Math.Abs(10 - solution) + 1.0;
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

    private sealed class SumToTargetConstructionModel :
        IAntColonyConstructionModel<
            int,int,int,ThreeCandidateEnumerator>
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

    private sealed class NeverStoppingCriterion : IStoppingCriterion
    {
        public string Name => "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }
}
