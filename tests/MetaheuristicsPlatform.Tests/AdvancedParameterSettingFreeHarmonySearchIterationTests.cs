using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedParameterSettingFreeHarmonySearchIterationTests
{
    [Fact]
    public void DescriptorUsesJeongParkGeemSimStableIdAndDoi()
    {
        var optimizer =
            new AdvancedParameterSettingFreeHarmonySearchIterationOptimizer();

        Assert.Equal(
            "advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.3390/app10072586");
    }

    [Fact]
    public void PublishedHmcrEquationIsMatched()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchIterationParameters
            {
                MaximumImprovisations = 20_000
            };

        const int dimension = 32;
        const int improvisation = 4_000;

        double expected =
            0.5 +
            (0.5 /
             (1.0 +
              Math.Exp(
                  -(
                      (10.0 * improvisation / 20_000.0) -
                      (5.0 / Math.Log(dimension))))));

        Assert.Equal(
            expected,
            parameters.GetHarmonyMemoryConsiderationRate(
                improvisation,
                dimension),
            12);
    }

    [Fact]
    public void PublishedParEquationIsMatched()
    {
        const double hmcr = 0.83;
        const int dimension = 32;

        double expected =
            hmcr /
            (1.0 +
             Math.Exp(
                 -(
                     (4.0 / dimension) -
                     2.0)));

        Assert.Equal(
            expected,
            AdvancedParameterSettingFreeHarmonySearchIterationParameters
                .GetPitchAdjustmentRate(
                    hmcr,
                    dimension),
            12);
    }

    [Fact]
    public void HmcrIncreasesWithIteration()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchIterationParameters
            {
                MaximumImprovisations = 1000
            };

        double early =
            parameters.GetHarmonyMemoryConsiderationRate(
                1,
                32);

        double late =
            parameters.GetHarmonyMemoryConsiderationRate(
                1000,
                32);

        Assert.True(
            late > early);

        Assert.InRange(
            early,
            0.5,
            1.0);

        Assert.InRange(
            late,
            0.5,
            1.0);
    }

    [Fact]
    public void OneDimensionUsesDocumentedRightHandLimit()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchIterationParameters();

        Assert.Equal(
            0.5,
            parameters.GetHarmonyMemoryConsiderationRate(
                1,
                1),
            12);
    }

    [Fact]
    public void PublicParametersDoNotExposeHmcrParOrOperationTypeMemory()
    {
        string[] names =
            typeof(AdvancedParameterSettingFreeHarmonySearchIterationParameters)
                .GetProperties()
                .Select(static property => property.Name)
                .ToArray();

        Assert.DoesNotContain(
            names,
            name =>
                name.Equals(
                    "HarmonyMemoryConsiderationRate",
                    StringComparison.Ordinal));

        Assert.DoesNotContain(
            names,
            name =>
                name.Equals(
                    "PitchAdjustmentRate",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new AdvancedParameterSettingFreeHarmonySearchIterationOptimizer().Optimize(
                CreateSphere(3),
                new AdvancedParameterSettingFreeHarmonySearchIterationParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 1
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
            "MaximumAdvancedParameterSettingFreeHarmonySearchIterationImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new AdvancedParameterSettingFreeHarmonySearchIterationOptimizer().Optimize(
                CreateSphere(2),
                new AdvancedParameterSettingFreeHarmonySearchIterationParameters
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
    }

    [Fact]
    public void MaximizationIsSupported()
    {
        OptimizationResult<double[]> result =
            new AdvancedParameterSettingFreeHarmonySearchIterationOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new AdvancedParameterSettingFreeHarmonySearchIterationParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 4
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
    }

    [Fact]
    public void FactoryCreatesSevenDistinctHarmonySearchIdentities()
    {
        string[] ids =
        [
            MetaheuristicFactory.Create<HarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.HarmonySearch).Descriptor.Id,
            MetaheuristicFactory.Create<ImprovedHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.ImprovedHarmonySearch).Descriptor.Id,
            MetaheuristicFactory.Create<GlobalBestHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.GlobalBestHarmonySearch).Descriptor.Id,
            MetaheuristicFactory.Create<SelfAdaptiveGlobalBestHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch).Descriptor.Id,
            MetaheuristicFactory.Create<NovelGlobalHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.NovelGlobalHarmonySearch).Descriptor.Id,
            MetaheuristicFactory.Create<ParameterSettingFreeHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.ParameterSettingFreeHarmonySearch).Descriptor.Id,
            MetaheuristicFactory.Create<AdvancedParameterSettingFreeHarmonySearchIterationOptimizer>(
                MetaheuristicAlgorithmIds.AdvancedParameterSettingFreeHarmonySearchIteration)
                .Descriptor.Id
        ];

        Assert.Equal(
            7,
            ids.Distinct(
                StringComparer.Ordinal).Count());
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new AdvancedParameterSettingFreeHarmonySearchIterationOptimizer().Optimize(
            CreateSphere(5),
            new AdvancedParameterSettingFreeHarmonySearchIterationParameters
            {
                HarmonyMemorySize = 10,
                MaximumImprovisations = 40,
                PitchAdjustmentBandwidthFractionOfRange = 0.001
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 12345UL },
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem CreateSphere(
        int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            Sphere);

    private static ContinuousOptimizationProblem CreateLinearProblem(
        OptimizationSense sense) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                1,
                -10.0,
                10.0),
            sense,
            static x => x[0]);

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0;
             i < x.Length;
             i++)
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
