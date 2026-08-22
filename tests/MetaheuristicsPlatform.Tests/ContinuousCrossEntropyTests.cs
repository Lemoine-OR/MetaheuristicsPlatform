using MetaheuristicsPlatform.Algorithms.CrossEntropy;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ContinuousCrossEntropyTests
{
    [Fact]
    public void DescriptorUsesContinuousCeStableIdAndScientificDoi()
    {
        var optimizer =
            new ContinuousCrossEntropyOptimizer();

        Assert.Equal(
            "cross-entropy-continuous-kroese-porotsky-rubinstein-2006",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1007/s11009-006-9753-0");
    }

    [Fact]
    public void TwoCompleteIterationsUseExactlyTwoSamplePopulations()
    {
        OptimizationResult<double[]> result =
            new ContinuousCrossEntropyOptimizer().Optimize(
                CreateSphere(4),
                new ContinuousCrossEntropyParameters
                {
                    SampleCount = 10,
                    EliteFraction = 0.2,
                    MaximumIterations = 2,
                    MinimumStandardDeviation = 1e-14
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 17UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(20, result.Statistics.Evaluations);
        Assert.Equal(2, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumCrossEntropyIterations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetStopsInsideIterationWithoutDistributionUpdate()
    {
        OptimizationResult<double[]> result =
            new ContinuousCrossEntropyOptimizer().Optimize(
                CreateSphere(4),
                new ContinuousCrossEntropyParameters
                {
                    SampleCount = 10,
                    EliteFraction = 0.2,
                    MaximumIterations = 20,
                    MinimumStandardDeviation = 1e-14
                },
                new ArraySolutionCloner<double>(),
                new MaxEvaluationsStoppingCriterion(13),
                new OptimizationOptions { Seed = 23UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(13, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            RunDeterministic();

        OptimizationResult<double[]> second =
            RunDeterministic();

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(first.Statistics.Evaluations, second.Statistics.Evaluations);
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ContinuousCrossEntropyParameters
                {
                    SampleCount = 1
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ContinuousCrossEntropyParameters
                {
                    EliteFraction = 1.0
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ContinuousCrossEntropyParameters
                {
                    StandardDeviationSmoothingBase = 1.0
                }.Validate());
    }

    [Fact]
    public void InitialMeanMustBelongToBoundedDomain()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ContinuousCrossEntropyOptimizer().Optimize(
                    CreateSphere(2),
                    new ContinuousCrossEntropyParameters
                    {
                        SampleCount = 8,
                        MaximumIterations = 1,
                        InitialMean = [50.0, 0.0]
                    },
                    new ArraySolutionCloner<double>(),
                    new NeverStoppingCriterion(),
                    new OptimizationOptions { Seed = 3UL },
                    cancellationToken:
                        TestContext.Current.CancellationToken));
    }

    [Fact]
    public void FactoryCreatesContinuousCrossEntropy()
    {
        ContinuousCrossEntropyOptimizer optimizer =
            MetaheuristicFactory.Create<ContinuousCrossEntropyOptimizer>(
                MetaheuristicAlgorithmIds.ContinuousCrossEntropy);

        Assert.NotNull(optimizer);
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new ContinuousCrossEntropyOptimizer().Optimize(
            CreateSphere(6),
            new ContinuousCrossEntropyParameters
            {
                SampleCount = 30,
                EliteFraction = 0.2,
                MaximumIterations = 8,
                MinimumStandardDeviation = 1e-14
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 12345UL },
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem
        CreateSphere(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -10.0,
                10.0),
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
        public string Name => "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }
}
