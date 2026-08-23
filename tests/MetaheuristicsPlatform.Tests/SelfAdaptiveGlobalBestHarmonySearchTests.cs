using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class SelfAdaptiveGlobalBestHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesPanSuganthanTasgetirenLiangStableIdAndPrimaryDoi()
    {
        var optimizer =
            new SelfAdaptiveGlobalBestHarmonySearchOptimizer();

        Assert.Equal(
            "self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/j.amc.2010.01.088");
    }

    [Fact]
    public void DefaultsMatchPublishedSghsLearningSettings()
    {
        var parameters =
            new SelfAdaptiveGlobalBestHarmonySearchParameters();

        Assert.Equal(5, parameters.HarmonyMemorySize);
        Assert.Equal(0.98, parameters.InitialMeanHarmonyMemoryConsiderationRate, 12);
        Assert.Equal(0.9, parameters.InitialMeanPitchAdjustmentRate, 12);
        Assert.Equal(100, parameters.LearningPeriod);
        Assert.Equal(0.0005, parameters.MinimumPitchAdjustmentBandwidth, 12);
        Assert.Equal(0.1, parameters.MaximumPitchAdjustmentBandwidthFractionOfRange, 12);
        Assert.Equal(
            0.01,
            SelfAdaptiveGlobalBestHarmonySearchParameters
                .HarmonyMemoryConsiderationRateStandardDeviation,
            12);
        Assert.Equal(
            0.05,
            SelfAdaptiveGlobalBestHarmonySearchParameters
                .PitchAdjustmentRateStandardDeviation,
            12);
    }

    [Fact]
    public void BandwidthScheduleMatchesPublishedPiecewiseRule()
    {
        var parameters =
            new SelfAdaptiveGlobalBestHarmonySearchParameters
            {
                MaximumImprovisations = 100,
                MinimumPitchAdjustmentBandwidth = 0.0005,
                MaximumPitchAdjustmentBandwidthFractionOfRange = 0.1
            };

        double maximum =
            1.0;

        double expectedAtOne =
            maximum -
            (((maximum - 0.0005) / 100.0) * 2.0);

        Assert.Equal(
            expectedAtOne,
            parameters.GetPitchAdjustmentBandwidth(1, 10.0),
            12);

        Assert.Equal(
            0.0005,
            parameters.GetPitchAdjustmentBandwidth(50, 10.0),
            12);

        Assert.Equal(
            0.0005,
            parameters.GetPitchAdjustmentBandwidth(100, 10.0),
            12);
    }

    [Fact]
    public void SampledRatesRemainInsidePublishedRanges()
    {
        var callback =
            new StateCaptureCallback();

        _ =
            new SelfAdaptiveGlobalBestHarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new SelfAdaptiveGlobalBestHarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 20,
                    LearningPeriod = 5
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 451UL },
                callback,
                TestContext.Current.CancellationToken);

        Assert.NotEmpty(
            callback.CompletedStates);

        Assert.All(
            callback.CompletedStates,
            state =>
            {
                Assert.InRange(
                    state.HarmonyMemoryConsiderationRate,
                    0.9,
                    1.0);

                Assert.InRange(
                    state.PitchAdjustmentRate,
                    0.0,
                    1.0);
            });
    }

    [Fact]
    public void EmptySuccessfulLearningPeriodPreservesMeans()
    {
        var callback =
            new StateCaptureCallback();

        _ =
            new SelfAdaptiveGlobalBestHarmonySearchOptimizer().Optimize(
                CreateConstantProblem(2),
                new SelfAdaptiveGlobalBestHarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 2,
                    LearningPeriod = 2
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 77UL },
                callback,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            2,
            callback.CompletedStates.Count);

        SelfAdaptiveGlobalBestHarmonySearchState finalState =
            callback.CompletedStates[^1];

        Assert.Equal(
            2,
            finalState.Iteration);

        Assert.Equal(
            1,
            finalState.LearningUpdates);

        Assert.Equal(
            0,
            finalState.LastCompletedLearningPeriodSuccessfulSamples);

        Assert.Equal(
            0.98,
            finalState.MeanHarmonyMemoryConsiderationRate,
            12);

        Assert.Equal(
            0.9,
            finalState.MeanPitchAdjustmentRate,
            12);
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new SelfAdaptiveGlobalBestHarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new SelfAdaptiveGlobalBestHarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 1,
                    LearningPeriod = 10
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
            "MaximumSelfAdaptiveGlobalBestHarmonySearchImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new SelfAdaptiveGlobalBestHarmonySearchOptimizer().Optimize(
                CreateSphere(2),
                new SelfAdaptiveGlobalBestHarmonySearchParameters
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
    public void FactoryCreatesFourDistinctHarmonySearchIdentities()
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

        SelfAdaptiveGlobalBestHarmonySearchOptimizer selfAdaptive =
            MetaheuristicFactory.Create<SelfAdaptiveGlobalBestHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch);

        string[] ids =
        [
            canonical.Descriptor.Id,
            improved.Descriptor.Id,
            globalBest.Descriptor.Id,
            selfAdaptive.Descriptor.Id
        ];

        Assert.Equal(
            4,
            ids.Distinct(StringComparer.Ordinal).Count());
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new SelfAdaptiveGlobalBestHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            new SelfAdaptiveGlobalBestHarmonySearchParameters
            {
                HarmonyMemorySize = 5,
                MaximumImprovisations = 20,
                LearningPeriod = 5
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

    private static ContinuousOptimizationProblem CreateConstantProblem(
        int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            static _ => 0.0);

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

    private sealed class StateCaptureCallback :
        IOptimizationCallback<double[]>
    {
        public OptimizationCallbackEvents Events =>
            OptimizationCallbackEvents.IterationCompleted;

        public List<SelfAdaptiveGlobalBestHarmonySearchState>
            CompletedStates { get; } =
            [];

        public void OnEvent(
            in OptimizationEvent<double[]> optimizationEvent)
        {
            if (optimizationEvent.AlgorithmData is
                SelfAdaptiveGlobalBestHarmonySearchState state &&
                state.Phase ==
                    HarmonySearchPhase.CompletedImprovisation)
            {
                CompletedStates.Add(
                    state);
            }
        }
    }
}
