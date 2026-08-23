using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class HarmonySearchTests
{
    [Fact]
    public void DescriptorUsesCanonicalStableIdPrimaryDoiAndOtherFamily()
    {
        var optimizer =
            new HarmonySearchOptimizer();

        Assert.Equal(
            "harmony-search-geem-kim-loganathan-2001",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1177/003754970107600201");
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new HarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new HarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 1,
                    HarmonyMemoryConsiderationRate = 1.0,
                    PitchAdjustmentRate = 0.0,
                    PitchAdjustmentBandwidth = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 17UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            5,
            result.Statistics.Evaluations);

        Assert.Equal(
            1,
            result.Statistics.Iterations);

        Assert.Equal(
            "MaximumHarmonySearchImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new HarmonySearchOptimizer().Optimize(
                CreateSphere(2),
                new HarmonySearchParameters
                {
                    HarmonyMemorySize = 5,
                    MaximumImprovisations = 10
                },
                new ArraySolutionCloner<double>(),
                new MaxEvaluationsStoppingCriterion(3),
                new OptimizationOptions { Seed = 23UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            3,
            result.Statistics.Evaluations);

        Assert.Equal(
            0,
            result.Statistics.Iterations);

        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void MaximizationUsesObjectiveSenseSymmetrically()
    {
        OptimizationResult<double[]> result =
            new HarmonySearchOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new HarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 4,
                    HarmonyMemoryConsiderationRate = 0.0,
                    PitchAdjustmentRate = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 31UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            7,
            result.Statistics.Evaluations);

        Assert.Equal(
            4,
            result.Statistics.Iterations);
    }

    [Fact]
    public void RandomOnlyImprovisationKeepsBestSolutionInsideBounds()
    {
        OptimizationResult<double[]> result =
            new HarmonySearchOptimizer().Optimize(
                CreateSphere(5),
                new HarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 8,
                    HarmonyMemoryConsiderationRate = 0.0,
                    PitchAdjustmentRate = 1.0,
                    PitchAdjustmentBandwidth = 100.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 41UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.All(
            result.BestSolution,
            coordinate =>
                Assert.InRange(
                    coordinate,
                    -5.0,
                    5.0));
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
                new HarmonySearchParameters
                {
                    HarmonyMemorySize = 0
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HarmonySearchParameters
                {
                    HarmonyMemoryConsiderationRate = 1.01
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HarmonySearchParameters
                {
                    PitchAdjustmentRate = -0.01
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HarmonySearchParameters
                {
                    PitchAdjustmentBandwidth = -1.0
                }.Validate());
    }

    [Fact]
    public void FactoryCreatesHarmonySearch()
    {
        HarmonySearchOptimizer optimizer =
            MetaheuristicFactory.Create<HarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.HarmonySearch);

        Assert.NotNull(
            optimizer);
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new HarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            new HarmonySearchParameters
            {
                HarmonyMemorySize = 8,
                MaximumImprovisations = 20,
                HarmonyMemoryConsiderationRate = 0.9,
                PitchAdjustmentRate = 0.3,
                PitchAdjustmentBandwidth = 0.1
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