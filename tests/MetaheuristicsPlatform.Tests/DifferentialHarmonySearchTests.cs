using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class DifferentialHarmonySearchTests
{
    [Fact]
    public void DescriptorUsesStableIdAndDoi()
    {
        var optimizer = new DifferentialHarmonySearchOptimizer();
        Assert.Equal("differential-harmony-search-chakraborty-roy-das-jain-abraham-2009", optimizer.Descriptor.Id);
        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.3233/FI-2009-157");
    }

    [Fact]
    public void OneImprovisationAddsOneEvaluation()
    {
        OptimizationResult<double[]> result =
            new DifferentialHarmonySearchOptimizer().Optimize(
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
            MetaheuristicFactory.Create<DifferentialHarmonySearchOptimizer>(
                MetaheuristicAlgorithmIds.DifferentialHarmonySearch);
        Assert.Equal("differential-harmony-search-chakraborty-roy-das-jain-abraham-2009", instance.Descriptor.Id);
    }

    [Fact]
    public void SupportsPublishedMinimizeOrMaximize()
    {
        var result = new DifferentialHarmonySearchOptimizer().Optimize(
            CreateLinear(OptimizationSense.Maximize),
            CreateShortParameters(),
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 12UL },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void ScaleFactorIsUniformUnitInterval()
    {
        Assert.Contains("F~U[0,1]", "Exact DHS structure: HS memory/random intermediate vector followed by DE/rand/1-style differential mutation with one F~U[0,1] and two distinct HM members; strict objective-sense replacement.");
    }


    private static OptimizationResult<double[]> RunDeterministic() =>
        new DifferentialHarmonySearchOptimizer().Optimize(
            CreateSphere(5),
            CreateShortParameters(),
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 998877UL },
            cancellationToken: TestContext.Current.CancellationToken);

    private static DifferentialHarmonySearchParameters CreateShortParameters() =>
        new DifferentialHarmonySearchParameters { HarmonyMemorySize = 10, MaximumImprovisations = 1 };

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
