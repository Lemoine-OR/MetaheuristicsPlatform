using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class NovelGlobalHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesZouGaoWuLiStableIdAndPrimaryDoi()
    {
        var optimizer =
            new NovelGlobalHarmonySearchOptimizer();

        Assert.Equal(
            "novel-global-harmony-search-zou-gao-wu-li-2010",
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Other));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1016/j.cie.2009.11.003");
    }

    [Fact]
    public void DefaultsMatchCanonicalContinuousNghsSettings()
    {
        var parameters =
            new NovelGlobalHarmonySearchParameters();

        Assert.Equal(
            5,
            parameters.HarmonyMemorySize);

        Assert.Equal(
            0.005,
            parameters.MutationProbability,
            12);
    }

    [Fact]
    public void PublicParametersExcludeHmcrParAndBandwidth()
    {
        string[] names =
            typeof(NovelGlobalHarmonySearchParameters)
                .GetProperties()
                .Select(static property => property.Name)
                .ToArray();

        Assert.DoesNotContain(
            names,
            name =>
                name.Contains(
                    "HarmonyMemoryConsiderationRate",
                    StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            names,
            name =>
                name.Contains(
                    "PitchAdjustmentRate",
                    StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            names,
            name =>
                name.Contains(
                    "Bandwidth",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ReplacementIsUnconditionalEvenWithoutStrictImprovement()
    {
        var callback =
            new StateCaptureCallback();

        _ =
            new NovelGlobalHarmonySearchOptimizer().Optimize(
                CreateConstantProblem(2),
                new NovelGlobalHarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 1,
                    MutationProbability = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 901UL },
                callback,
                TestContext.Current.CancellationToken);

        NovelGlobalHarmonySearchState state =
            Assert.Single(
                callback.CompletedStates);

        Assert.True(
            state.UnconditionallyReplacedWorstHarmony);

        Assert.False(
            state.CandidateWasStrictlyBetterThanReplacedWorst);

        Assert.Equal(
            0.0,
            state.CandidateFitness);

        Assert.Equal(
            0.0,
            state.ReplacedWorstFitness);
    }

    [Fact]
    public void MutationProbabilityOneMutatesEveryCoordinate()
    {
        var callback =
            new StateCaptureCallback();

        _ =
            new NovelGlobalHarmonySearchOptimizer().Optimize(
                CreateSphere(4),
                new NovelGlobalHarmonySearchParameters
                {
                    HarmonyMemorySize = 3,
                    MaximumImprovisations = 1,
                    MutationProbability = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 902UL },
                callback,
                TestContext.Current.CancellationToken);

        NovelGlobalHarmonySearchState state =
            Assert.Single(
                callback.CompletedStates);

        Assert.Equal(
            4,
            state.MutatedCoordinateCount);
    }

    [Fact]
    public void OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new NovelGlobalHarmonySearchOptimizer().Optimize(
                CreateSphere(3),
                new NovelGlobalHarmonySearchParameters
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
            "MaximumNovelGlobalHarmonySearchImprovisations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetMayStopDuringHarmonyMemoryInitialization()
    {
        OptimizationResult<double[]> result =
            new NovelGlobalHarmonySearchOptimizer().Optimize(
                CreateSphere(2),
                new NovelGlobalHarmonySearchParameters
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
    public void MaximizationIsSupported()
    {
        OptimizationResult<double[]> result =
            new NovelGlobalHarmonySearchOptimizer().Optimize(
                CreateLinearProblem(
                    OptimizationSense.Maximize),
                new NovelGlobalHarmonySearchParameters
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

        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
    }

    [Fact]
    public void InvalidMutationProbabilityIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new NovelGlobalHarmonySearchParameters
                {
                    MutationProbability = -0.01
                }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new NovelGlobalHarmonySearchParameters
                {
                    MutationProbability = 1.01
                }.Validate());
    }

    [Fact]
    public void FactoryCreatesFiveDistinctHarmonySearchIdentities()
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
                .Descriptor.Id
        ];

        Assert.Equal(
            5,
            ids.Distinct(
                StringComparer.Ordinal).Count());
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new NovelGlobalHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            new NovelGlobalHarmonySearchParameters
            {
                HarmonyMemorySize = 5,
                MaximumImprovisations = 20,
                MutationProbability = 0.005
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

        public List<NovelGlobalHarmonySearchState>
            CompletedStates { get; } =
            [];

        public void OnEvent(
            in OptimizationEvent<double[]> optimizationEvent)
        {
            if (optimizationEvent.AlgorithmData is
                NovelGlobalHarmonySearchState state &&
                state.Phase ==
                    HarmonySearchPhase.CompletedImprovisation)
            {
                CompletedStates.Add(
                    state);
            }
        }
    }
}
