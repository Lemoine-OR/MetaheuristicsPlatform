using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ImprovedHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesMahdaviStableIdAndPrimaryDoi()
    {
        var optimizer =
            new ImprovedHarmonySearchOptimizer();

        Assert.Equal(
            "improved-harmony-search-mahdavi-fesanghary-damangir-2007",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/j.amc.2006.11.033");
    }

    [Fact]
    public void DynamicSchedulesMatchPublishedEquations()
    {
        var parameters =
            new ImprovedHarmonySearchParameters
            {
                MaximumImprovisations = 100,
                MinimumPitchAdjustmentRate = 0.01,
                MaximumPitchAdjustmentRate = 0.99,
                MinimumPitchAdjustmentBandwidth = 0.0001,
                MaximumPitchAdjustmentBandwidth = 1.0
            };

        double expectedParAtOne =
            0.01 +
            ((0.99 - 0.01) / 100.0);

        double expectedBwAtOne =
            Math.Exp(
                Math.Log(0.0001 / 1.0) /
                100.0);

        Assert.Equal(
            expectedParAtOne,
            parameters.GetPitchAdjustmentRate(1),
            12);

        Assert.Equal(
            expectedBwAtOne,
            parameters.GetPitchAdjustmentBandwidth(1),
            12);

        Assert.Equal(
            0.99,
            parameters.GetPitchAdjustmentRate(100),
            12);

        Assert.Equal(
            0.0001,
            parameters.GetPitchAdjustmentBandwidth(100),
            12);
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new ImprovedHarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new ImprovedHarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 1,
                    HarmonyMemoryConsiderationRate = 1.0,
                    MinimumPitchAdjustmentRate = 0.0,
                    MaximumPitchAdjustmentRate = 0.0,
                    MinimumPitchAdjustmentBandwidth = 0.1,
                    MaximumPitchAdjustmentBandwidth = 0.1
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
            "MaximumImprovedHarmonySearchImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new ImprovedHarmonySearchOptimizer().Optimize(
                CreateSphere(2),
                new ImprovedHarmonySearchParameters
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
            new ImprovedHarmonySearchOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new ImprovedHarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 4,
                    HarmonyMemoryConsiderationRate = 0.0
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
    public void InvalidScheduleParametersAreRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new ImprovedHarmonySearchParameters
                {
                    MinimumPitchAdjustmentRate = 0.9,
                    MaximumPitchAdjustmentRate = 0.1
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ImprovedHarmonySearchParameters
                {
                    MinimumPitchAdjustmentBandwidth = 0.0
                }.Validate());

        Assert.Throws<ArgumentException>(
            () =>
                new ImprovedHarmonySearchParameters
                {
                    MinimumPitchAdjustmentBandwidth = 2.0,
                    MaximumPitchAdjustmentBandwidth = 1.0
                }.Validate());
    }

    [Fact]
    public void FactoryCreatesImprovedHarmonySearchAndCanonicalHsRemainsDistinct()
    {
        ImprovedHarmonySearchOptimizer improved =
            MetaheuristicFactory.Create<ImprovedHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.ImprovedHarmonySearch);

        HarmonySearchOptimizer canonical =
            MetaheuristicFactory.Create<HarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.HarmonySearch);

        Assert.NotNull(
            improved);

        Assert.NotNull(
            canonical);

        Assert.NotEqual(
            improved.Descriptor.Id,
            canonical.Descriptor.Id);
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new ImprovedHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            new ImprovedHarmonySearchParameters
            {
                HarmonyMemorySize = 8,
                MaximumImprovisations = 20,
                HarmonyMemoryConsiderationRate = 0.9,
                MinimumPitchAdjustmentRate = 0.01,
                MaximumPitchAdjustmentRate = 0.99,
                MinimumPitchAdjustmentBandwidth = 0.0001,
                MaximumPitchAdjustmentBandwidth = 0.5
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
