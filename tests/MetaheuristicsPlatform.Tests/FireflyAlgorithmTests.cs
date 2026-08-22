using MetaheuristicsPlatform.Algorithms.Firefly;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class FireflyAlgorithmTests
{
    [Fact]
    public void DescriptorUsesCanonicalStableIdAndPrimaryDoi()
    {
        var optimizer =
            new FireflyOptimizer();

        Assert.Equal(
            "firefly-algorithm-yang-2009",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1007/978-3-642-04944-6_14");
    }

    [Fact]
    public void TwoFirefliesWithPureAttractionPerformExactlyOneMove()
    {
        OptimizationResult<double[]> result =
            new FireflyOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Minimize),
                new FireflyParameters
                {
                    PopulationSize = 2,
                    MaximumIterations = 1,
                    BaseAttractiveness = 1.0,
                    LightAbsorptionCoefficient = 0.0,
                    RandomizationAmplitude = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 17UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumFireflyIterations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetStopsInsidePairwiseSweepWithoutCountingIteration()
    {
        OptimizationResult<double[]> result =
            new FireflyOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Minimize),
                new FireflyParameters
                {
                    PopulationSize = 2,
                    MaximumIterations = 10,
                    BaseAttractiveness = 1.0,
                    LightAbsorptionCoefficient = 0.0,
                    RandomizationAmplitude = 0.0
                },
                new ArraySolutionCloner<double>(),
                new MaxEvaluationsStoppingCriterion(3),
                new OptimizationOptions { Seed = 23UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void ConstantBrightnessCompletesIterationWithoutAttractionEvaluations()
    {
        OptimizationResult<double[]> result =
            new FireflyOptimizer().Optimize(
                CreateConstantProblem(3),
                new FireflyParameters
                {
                    PopulationSize = 4,
                    MaximumIterations = 1
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 7UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(4, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void MaximizationUsesObjectiveSenseSymmetrically()
    {
        OptimizationResult<double[]> result =
            new FireflyOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new FireflyParameters
                {
                    PopulationSize = 2,
                    MaximumIterations = 1,
                    BaseAttractiveness = 1.0,
                    LightAbsorptionCoefficient = 0.0,
                    RandomizationAmplitude = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 31UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            RunDeterministic();

        OptimizationResult<double[]> second =
            RunDeterministic();

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);

        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new FireflyParameters
                {
                    PopulationSize = 1
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new FireflyParameters
                {
                    LightAbsorptionCoefficient = -1.0
                }.Validate());
    }

    [Fact]
    public void FactoryCreatesFireflyAlgorithm()
    {
        FireflyOptimizer optimizer =
            MetaheuristicFactory.Create<FireflyOptimizer>(
                MetaheuristicAlgorithmIds.Firefly);

        Assert.NotNull(
            optimizer);
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new FireflyOptimizer().Optimize(
            CreateSphere(5),
            new FireflyParameters
            {
                PopulationSize = 8,
                MaximumIterations = 5,
                BaseAttractiveness = 1.0,
                LightAbsorptionCoefficient = 0.5,
                RandomizationAmplitude = 0.2
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 12345UL },
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem
        CreateLinearProblem(OptimizationSense sense) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                1,
                -10.0,
                10.0),
            sense,
            static x => x[0]);

    private static ContinuousOptimizationProblem
        CreateConstantProblem(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -10.0,
                10.0),
            OptimizationSense.Minimize,
            static _ => 1.0);

    private static ContinuousOptimizationProblem
        CreateSphere(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            Sphere);

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            sum +=
                x[i] *
                x[i];
        }

        return sum;
    }

    private sealed class NeverStoppingCriterion :
        IStoppingCriterion
    {
        public string Name =>
            "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(
                Name);
    }
}
