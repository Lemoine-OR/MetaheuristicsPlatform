using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class GlobalBestHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesOmranMahdaviStableIdAndPrimaryDoi()
    {
        var optimizer =
            new GlobalBestHarmonySearchOptimizer();

        Assert.Equal(
            "global-best-harmony-search-omran-mahdavi-2008",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/j.amc.2007.09.004");
    }

    [Fact]
    public void DynamicParScheduleMatchesPublishedEquation()
    {
        var parameters =
            new GlobalBestHarmonySearchParameters
            {
                MaximumImprovisations = 100,
                MinimumPitchAdjustmentRate = 0.01,
                MaximumPitchAdjustmentRate = 0.99
            };

        double expectedAtOne =
            0.01 +
            ((0.99 - 0.01) / 100.0);

        Assert.Equal(
            expectedAtOne,
            parameters.GetPitchAdjustmentRate(1),
            12);

        Assert.Equal(
            0.99,
            parameters.GetPitchAdjustmentRate(100),
            12);
    }

    [Fact]
    public void PublicParametersContainNoBandwidth()
    {
        string[] names =
            typeof(GlobalBestHarmonySearchParameters)
                .GetProperties()
                .Select(static property => property.Name)
                .ToArray();

        Assert.DoesNotContain(
            names,
            name =>
                name.Contains(
                    "Bandwidth",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new GlobalBestHarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new GlobalBestHarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 1,
                    HarmonyMemoryConsiderationRate = 1.0,
                    MinimumPitchAdjustmentRate = 1.0,
                    MaximumPitchAdjustmentRate = 1.0
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
            "MaximumGlobalBestHarmonySearchImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new GlobalBestHarmonySearchOptimizer().Optimize(
                CreateSphere(2),
                new GlobalBestHarmonySearchParameters
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
            new GlobalBestHarmonySearchOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new GlobalBestHarmonySearchParameters
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
    public void InvalidParBoundsAreRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new GlobalBestHarmonySearchParameters
                {
                    MinimumPitchAdjustmentRate = 0.9,
                    MaximumPitchAdjustmentRate = 0.1
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new GlobalBestHarmonySearchParameters
                {
                    HarmonyMemoryConsiderationRate = 1.1
                }.Validate());
    }

    [Fact]
    public void FactoryCreatesThreeDistinctHarmonySearchIdentities()
    {
        HarmonySearchOptimizer canonical =
            MetaheuristicFactory.Create<HarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.HarmonySearch);

        ImprovedHarmonySearchOptimizer improved =
            MetaheuristicFactory.Create<ImprovedHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.ImprovedHarmonySearch);

        GlobalBestHarmonySearchOptimizer globalBest =
            MetaheuristicFactory.Create<GlobalBestHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.GlobalBestHarmonySearch);

        Assert.NotEqual(
            canonical.Descriptor.Id,
            improved.Descriptor.Id);

        Assert.NotEqual(
            canonical.Descriptor.Id,
            globalBest.Descriptor.Id);

        Assert.NotEqual(
            improved.Descriptor.Id,
            globalBest.Descriptor.Id);
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new GlobalBestHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            new GlobalBestHarmonySearchParameters
            {
                HarmonyMemorySize = 5,
                MaximumImprovisations = 20,
                HarmonyMemoryConsiderationRate = 0.9,
                MinimumPitchAdjustmentRate = 0.01,
                MaximumPitchAdjustmentRate = 0.99
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
