using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ParameterSettingFreeHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesGeemSimStableIdAndPrimaryDoi()
    {
        var optimizer =
            new ParameterSettingFreeHarmonySearchOptimizer();

        Assert.Equal(
            "parameter-setting-free-harmony-search-geem-sim-2010",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/j.amc.2010.09.049");
    }

    [Fact]
    public void DefaultsPreserveConventionalRehearsalSettings()
    {
        var parameters =
            new ParameterSettingFreeHarmonySearchParameters();

        Assert.Equal(
            30,
            parameters.HarmonyMemorySize);

        Assert.Equal(
            3,
            parameters.RehearsalMemoryCycles);

        Assert.Equal(
            0.5,
            ParameterSettingFreeHarmonySearchParameters
                .RehearsalHarmonyMemoryConsiderationRate,
            12);

        Assert.Equal(
            0.5,
            ParameterSettingFreeHarmonySearchParameters
                .RehearsalPitchAdjustmentRate,
            12);
    }

    [Fact]
    public void RehearsalCountIsMemoryCyclesTimesHmsCappedByNi()
    {
        Assert.Equal(
            12,
            new ParameterSettingFreeHarmonySearchParameters
            {
                HarmonyMemorySize = 4,
                RehearsalMemoryCycles = 3,
                MaximumImprovisations = 20
            }.GetRehearsalImprovisations());

        Assert.Equal(
            5,
            new ParameterSettingFreeHarmonySearchParameters
            {
                HarmonyMemorySize = 4,
                RehearsalMemoryCycles = 3,
                MaximumImprovisations = 5
            }.GetRehearsalImprovisations());
    }

    [Fact]
    public void ConstantProblemLeavesInitialRandomOtmAndYieldsZeroAdaptiveRates()
    {
        var callback =
            new StateCaptureCallback();

        _ =
            new ParameterSettingFreeHarmonySearchOptimizer().Optimize(
                CreateConstantProblem(2),
                new ParameterSettingFreeHarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 4,
                    RehearsalMemoryCycles = 1,
                    PitchAdjustmentBandwidthFractionOfRange = 0.001
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 404UL },
                callback,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            4,
            callback.CompletedStates.Count);

        ParameterSettingFreeHarmonySearchState performanceState =
            callback.CompletedStates[^1];

        Assert.Equal(
            ParameterSettingFreeHarmonySearchStage.Performance,
            performanceState.Stage);

        Assert.Equal(
            6,
            performanceState.RandomOperationCount);

        Assert.Equal(
            0,
            performanceState.MemoryOperationCount);

        Assert.Equal(
            0,
            performanceState.PitchOperationCount);

        Assert.Equal(
            0.0,
            performanceState.MinimumHarmonyMemoryConsiderationRate,
            12);

        Assert.Equal(
            0.0,
            performanceState.MaximumHarmonyMemoryConsiderationRate,
            12);

        Assert.Equal(
            0.0,
            performanceState.MinimumPitchAdjustmentRate,
            12);

        Assert.Equal(
            0.0,
            performanceState.MaximumPitchAdjustmentRate,
            12);
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new ParameterSettingFreeHarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new ParameterSettingFreeHarmonySearchParameters
                {
                    HarmonyMemorySize = 4,
                    MaximumImprovisations = 1,
                    RehearsalMemoryCycles = 1
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
            "MaximumParameterSettingFreeHarmonySearchImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new ParameterSettingFreeHarmonySearchOptimizer().Optimize(
                CreateSphere(2),
                new ParameterSettingFreeHarmonySearchParameters
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
            new ParameterSettingFreeHarmonySearchOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new ParameterSettingFreeHarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 4,
                    RehearsalMemoryCycles = 1
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
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ParameterSettingFreeHarmonySearchParameters
                {
                    RehearsalMemoryCycles = 0
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ParameterSettingFreeHarmonySearchParameters
                {
                    PitchAdjustmentBandwidthFractionOfRange =
                        double.NaN
                }.Validate());
    }

    [Fact]
    public void FactoryCreatesSixDistinctHarmonySearchIdentities()
    {
        string[] ids =
        [
            MetaheuristicFactory
                .Create<HarmonySearchOptimizer>(
                    MetaheuristicAlgorithmIds.HarmonySearch)
                .Descriptor.Id,
            MetaheuristicFactory
                .Create<ImprovedHarmonySearchOptimizer>(
                    MetaheuristicAlgorithmIds.ImprovedHarmonySearch)
                .Descriptor.Id,
            MetaheuristicFactory
                .Create<GlobalBestHarmonySearchOptimizer>(
                    MetaheuristicAlgorithmIds.GlobalBestHarmonySearch)
                .Descriptor.Id,
            MetaheuristicFactory
                .Create<SelfAdaptiveGlobalBestHarmonySearchOptimizer>(
                    MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch)
                .Descriptor.Id,
            MetaheuristicFactory
                .Create<NovelGlobalHarmonySearchOptimizer>(
                    MetaheuristicAlgorithmIds.NovelGlobalHarmonySearch)
                .Descriptor.Id,
            MetaheuristicFactory
                .Create<ParameterSettingFreeHarmonySearchOptimizer>(
                    MetaheuristicAlgorithmIds.ParameterSettingFreeHarmonySearch)
                .Descriptor.Id
        ];

        Assert.Equal(
            6,
            ids.Distinct(
                StringComparer.Ordinal).Count());
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new ParameterSettingFreeHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            new ParameterSettingFreeHarmonySearchParameters
            {
                HarmonyMemorySize = 10,
                MaximumImprovisations = 40,
                RehearsalMemoryCycles = 1,
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

    private static ContinuousOptimizationProblem CreateConstantProblem(
        int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            static _ => 0.0);

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

    private sealed class StateCaptureCallback :
        IOptimizationCallback<double[]>
    {
        public OptimizationCallbackEvents Events =>
            OptimizationCallbackEvents.IterationCompleted;

        public List<ParameterSettingFreeHarmonySearchState>
            CompletedStates { get; } =
            [];

        public void OnEvent(
            in OptimizationEvent<double[]> optimizationEvent)
        {
            if (optimizationEvent.AlgorithmData is
                ParameterSettingFreeHarmonySearchState state &&
                state.Phase ==
                    HarmonySearchPhase.CompletedImprovisation)
            {
                CompletedStates.Add(
                    state);
            }
        }
    }
}
