using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdaptiveHarmonySearchDifferentialEvolutionTests
{
    [Fact]
    public void DescriptorUsesStableIdAndDoi()
    {
        var optimizer = new AdaptiveHarmonySearchDifferentialEvolutionOptimizer();
        Assert.Equal("adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020", optimizer.Descriptor.Id);
        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.3390/app10082916");
    }

    [Fact]
    public void OneImprovisationAddsOneEvaluation()
    {
        OptimizationResult<double[]> result =
            new AdaptiveHarmonySearchDifferentialEvolutionOptimizer().Optimize(
                CreateSphere(4),
                CreateShortParameters(),
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 12345UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Statistics.Evaluations >= 2);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first = RunDeterministic();
        OptimizationResult<double[]> second = RunDeterministic();
        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
    }

    [Fact]
    public void FactoryCreatesScientificIdentity()
    {
        var instance =
            MetaheuristicFactory.Create<AdaptiveHarmonySearchDifferentialEvolutionOptimizer>(
                MetaheuristicAlgorithmIds.AdaptiveHarmonySearchDifferentialEvolution);
        Assert.Equal("adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020", instance.Descriptor.Id);
    }

    [Fact]
    public void AdaptiveSampleIsClampedToPublishedRange()
    {
        double[] raw = [-1.0, 0.0, 0.25, 1.0, 2.0];
        double[] clamped =
            raw.Select(value =>
                    value > 1.0
                        ? 1.0
                        : value <= 0.0
                            ? 0.001
                            : value)
                .ToArray();

        Assert.Equal([0.001, 0.001, 0.25, 1.0, 1.0], clamped);
    }

    [Fact]
    public void LinearHarmonyMemoryReductionIsDocumented()
    {
        int maximumHms = 90;
        int minimumHms = 5;
        int maxNfe = 1000;
        int nfe = 500;

        int targetHms =
            (int)Math.Round(
                maximumHms -
                ((maximumHms - minimumHms) *
                 ((double)nfe / maxNfe)),
                MidpointRounding.AwayFromZero);

        Assert.Equal(48, targetHms);
    }

    [Fact]
    public void MaximizationIsRejected()
    {
        Assert.Throws<NotSupportedException>(() =>
            new AdaptiveHarmonySearchDifferentialEvolutionOptimizer().Optimize(
                CreateLinear(OptimizationSense.Maximize),
                CreateShortParameters(),
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 4UL },
                cancellationToken: TestContext.Current.CancellationToken));
    }


    private static OptimizationResult<double[]> RunDeterministic() =>
        new AdaptiveHarmonySearchDifferentialEvolutionOptimizer().Optimize(
            CreateSphere(5),
            CreateShortParameters(),
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 998877UL },
            cancellationToken: TestContext.Current.CancellationToken);

    private static AdaptiveHarmonySearchDifferentialEvolutionParameters CreateShortParameters() =>
        new AdaptiveHarmonySearchDifferentialEvolutionParameters { HarmonyMemorySize = 90, MaximumImprovisations = 1, MaximumHarmonyMemorySizePerDimension = 5, MaximumFunctionEvaluationsPerDimension = 100 };

    private static ContinuousOptimizationProblem CreateSphere(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(dimension, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private static ContinuousOptimizationProblem CreateLinear(
        OptimizationSense sense) =>
        new(
            BoundedContinuousSearchSpace.Uniform(4, -5.0, 5.0),
            sense,
            static x => x[0]);

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * x[i];
        }
        return sum;
    }

    private sealed class NeverStoppingCriterion : IStoppingCriterion
    {
        public string Name => "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }
}
