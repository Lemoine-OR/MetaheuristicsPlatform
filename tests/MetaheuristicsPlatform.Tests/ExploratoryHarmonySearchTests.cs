using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ExploratoryHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesStableIdAndDoi()
    {
        var optimizer = new ExploratoryHarmonySearchOptimizer();
        Assert.Equal("exploratory-harmony-search-das-mukhopadhyay-roy-abraham-panigrahi-2011", optimizer.Descriptor.Id);
        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.1109/TSMCB.2010.2046035");
    }

    [Fact]
    public void OneImprovisationAddsOneEvaluation()
    {
        OptimizationResult<double[]> result =
            new ExploratoryHarmonySearchOptimizer().Optimize(
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
            MetaheuristicFactory.Create<ExploratoryHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.ExploratoryHarmonySearch);
        Assert.Equal("exploratory-harmony-search-das-mukhopadhyay-roy-abraham-panigrahi-2011", instance.Descriptor.Id);
    }

    [Fact]
    public void PublishedDefaultKIsOnePointOneSeven()
    {
        Assert.Equal(
            1.17,
            new ExploratoryHarmonySearchParameters().StandardDeviationMultiplier,
            12);
    }

    [Fact]
    public void FineTuningWidthUsesHarmonyMemoryStandardDeviation()
    {
        double[] coordinate = [1.0, 2.0, 3.0, 4.0];
        double mean = coordinate.Average();
        double variance =
            coordinate
                .Select(value => (value - mean) * (value - mean))
                .Average();

        double expected =
            new ExploratoryHarmonySearchParameters()
                .StandardDeviationMultiplier *
            Math.Sqrt(variance);

        Assert.Equal(1.17 * Math.Sqrt(1.25), expected, 12);
    }


    private static OptimizationResult<double[]> RunDeterministic() =>
        new ExploratoryHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            CreateShortParameters(),
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 998877UL },
            cancellationToken: TestContext.Current.CancellationToken);

    private static ExploratoryHarmonySearchParameters CreateShortParameters() =>
        new ExploratoryHarmonySearchParameters { HarmonyMemorySize = 10, MaximumImprovisations = 1 };

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
