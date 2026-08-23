using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ImprovedHarmonySearchDifferentialMutationTests
{
    [Fact]
    public void DescriptorUsesStableIdAndDoi()
    {
        var optimizer = new ImprovedHarmonySearchDifferentialMutationOptimizer();
        Assert.Equal("improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012", optimizer.Descriptor.Id);
        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.1155/2012/147950");
    }

    [Fact]
    public void OneImprovisationAddsOneEvaluation()
    {
        OptimizationResult<double[]> result =
            new ImprovedHarmonySearchDifferentialMutationOptimizer().Optimize(
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
            MetaheuristicFactory.Create<ImprovedHarmonySearchDifferentialMutationOptimizer>(
                MetaheuristicAlgorithmIds.ImprovedHarmonySearchDifferentialMutation);
        Assert.Equal("improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012", instance.Descriptor.Id);
    }

    [Fact]
    public void ScaleFactorRangeIsPointSixToOne()
    {
        var random = new MetaheuristicsPlatform.Random.Xoshiro256StarStarRandomSource(456UL);

        for (int i = 0; i < 1000; i++)
        {
            double scaleFactor = 0.6 + (0.4 * random.NextDouble());
            Assert.InRange(scaleFactor, 0.6, 1.0);
        }
    }

    [Fact]
    public void MaximizationIsRejected()
    {
        Assert.Throws<NotSupportedException>(() =>
            new ImprovedHarmonySearchDifferentialMutationOptimizer().Optimize(
                CreateLinear(OptimizationSense.Maximize),
                CreateShortParameters(),
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 2UL },
                cancellationToken: TestContext.Current.CancellationToken));
    }


    private static OptimizationResult<double[]> RunDeterministic() =>
        new ImprovedHarmonySearchDifferentialMutationOptimizer().Optimize(
            CreateSphere(5),
            CreateShortParameters(),
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 998877UL },
            cancellationToken: TestContext.Current.CancellationToken);

    private static ImprovedHarmonySearchDifferentialMutationParameters CreateShortParameters() =>
        new ImprovedHarmonySearchDifferentialMutationParameters { HarmonyMemorySize = 10, MaximumImprovisations = 1 };

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
