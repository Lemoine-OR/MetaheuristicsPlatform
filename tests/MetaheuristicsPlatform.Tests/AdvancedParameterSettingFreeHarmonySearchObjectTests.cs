using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedParameterSettingFreeHarmonySearchObjectTests
{
    [Fact]
    public void DescriptorUsesObjectStableIdAndJeongParkGeemSimDoi()
    {
        var optimizer =
            new AdvancedParameterSettingFreeHarmonySearchObjectOptimizer();

        Assert.Equal(
            "advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.3390/app10072586");
    }

    [Fact]
    public void PublishedObjectHmcrEquationIsMatched()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchObjectParameters
            {
                TargetObjective = 0.0
            };

        const double lossStart = 10.0;
        const double lossMean = 5.0;
        const int dimension = 32;

        double argument =
            10.0 -
            (10.0 * ((lossMean - 0.0) / (lossStart - 0.0))) -
            (5.0 / Math.Log(dimension));

        double expected =
            0.5 +
            (0.5 /
             (1.0 +
              Math.Exp(-argument)));

        Assert.Equal(
            expected,
            parameters.GetObjectHarmonyMemoryConsiderationRate(
                lossMean,
                lossStart,
                dimension),
            12);
    }

    [Fact]
    public void PublishedParEquationIsMatched()
    {
        const double hmcr = 0.8;

        double expected =
            hmcr /
            (1.0 +
             Math.Exp(
                 -((4.0 / 32.0) - 2.0)));

        Assert.Equal(
            expected,
            AdvancedParameterSettingFreeHarmonySearchObjectParameters
                .GetPitchAdjustmentRate(
                    hmcr,
                    32),
            12);
    }

    [Fact]
    public void EquationNineUsesImprovementBranchAtThreshold()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchObjectParameters
            {
                TargetObjective = 0.0
            };

        Assert.Equal(
            0.01,
            parameters.GetAdaptiveBandwidthFraction(
                previousBlockMean: 8.0,
                currentBlockMean: 7.9,
                lossStart: 10.0),
            12);
    }

    [Fact]
    public void EquationNineUsesFallbackWhenImprovementIsTooSmall()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchObjectParameters
            {
                TargetObjective = 0.0
            };

        double expected =
            (1.0 -
             ((10.0 - 8.0) /
              10.0)) *
            0.1;

        Assert.Equal(
            expected,
            parameters.GetAdaptiveBandwidthFraction(
                previousBlockMean: 8.00001,
                currentBlockMean: 8.0,
                lossStart: 10.0),
            12);
    }

    [Fact]
    public void OneDimensionUsesDocumentedRightHandLimit()
    {
        var parameters =
            new AdvancedParameterSettingFreeHarmonySearchObjectParameters
            {
                TargetObjective = 0.0
            };

        Assert.Equal(
            0.5,
            parameters.GetObjectHarmonyMemoryConsiderationRate(
                lossMean: 5.0,
                lossStart: 10.0,
                dimension: 1),
            12);
    }

    [Fact]
    public void MaximizationIsRejectedBecausePublishedEquationIsForMinimum()
    {
        Assert.Throws<NotSupportedException>(
            () =>
                new AdvancedParameterSettingFreeHarmonySearchObjectOptimizer()
                    .Optimize(
                        CreateLinearProblem(
                            OptimizationSense.Maximize),
                        new AdvancedParameterSettingFreeHarmonySearchObjectParameters
                        {
                            TargetObjective = 0.0,
                            HarmonyMemorySize = 3,
                            MaximumImprovisations = 5
                        },
                        new ArraySolutionCloner<double>(),
                        new NeverStoppingCriterion(),
                        new OptimizationOptions { Seed = 10UL },
                        cancellationToken:
                            TestContext.Current.CancellationToken));
    }

    [Fact]
    public void TargetCanStopDuringInitialization()
    {
        OptimizationResult<double[]> result =
            new AdvancedParameterSettingFreeHarmonySearchObjectOptimizer()
                .Optimize(
                    CreateConstantProblem(
                        value: 0.0),
                    new AdvancedParameterSettingFreeHarmonySearchObjectParameters
                    {
                        TargetObjective = 0.0,
                        HarmonyMemorySize = 5,
                        MaximumImprovisations = 20
                    },
                    new ArraySolutionCloner<double>(),
                    new NeverStoppingCriterion(),
                    new OptimizationOptions { Seed = 11UL },
                    cancellationToken:
                        TestContext.Current.CancellationToken);

        Assert.Equal(
            1,
            result.Statistics.Evaluations);

        Assert.Equal(
            "AdvancedParameterSettingFreeHarmonySearchObjectTarget",
            result.StopDecision.Criterion);
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
    public void FactoryCreatesEightDistinctHarmonySearchIdentities()
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
                MetaheuristicAlgorithmIds.AdvancedParameterSettingFreeHarmonySearchIteration).Descriptor.Id,
            MetaheuristicFactory.Create<AdvancedParameterSettingFreeHarmonySearchObjectOptimizer>(
                MetaheuristicAlgorithmIds.AdvancedParameterSettingFreeHarmonySearchObject).Descriptor.Id
        ];

        Assert.Equal(
            8,
            ids.Distinct(
                StringComparer.Ordinal).Count());
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new AdvancedParameterSettingFreeHarmonySearchObjectOptimizer()
            .Optimize(
                CreateShiftedSphere(
                    dimension: 5,
                    offset: 100.0),
                new AdvancedParameterSettingFreeHarmonySearchObjectParameters
                {
                    TargetObjective = 100.0,
                    HarmonyMemorySize = 5,
                    MaximumImprovisations = 30,
                    InitialPitchAdjustmentBandwidthFractionOfRange = 0.001
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 12345UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem CreateShiftedSphere(
        int dimension,
        double offset) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            x =>
                offset +
                Sphere(x));

    private static ContinuousOptimizationProblem CreateConstantProblem(
        double value) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                2,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            _ => value);

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
