using MetaheuristicsPlatform.Algorithms.ArtificialBeeColony;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ArtificialBeeColonyTests
{
    [Fact]
    public void DescriptorUsesCanonicalStableIdAndDoi()
    {
        var optimizer =
            new ArtificialBeeColonyOptimizer();

        Assert.Equal(
            "artificial-bee-colony-karaboga-basturk-2007",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1007/s10898-007-9149-x");
    }

    [Fact]
    public void OneCycleUsesInitializationEmployedAndOnlookerEvaluations()
    {
        OptimizationResult<double[]> result =
            new ArtificialBeeColonyOptimizer().Optimize(
                CreateSphere(3),
                new ArtificialBeeColonyParameters
                {
                    FoodSourceCount = 4,
                    MaximumCycles = 1,
                    AbandonmentLimit = 1000
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 17UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(12, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumArtificialBeeColonyCycles",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetStopsInsideCycleWithoutOvershoot()
    {
        OptimizationResult<double[]> result =
            new ArtificialBeeColonyOptimizer().Optimize(
                CreateSphere(3),
                new ArtificialBeeColonyParameters
                {
                    FoodSourceCount = 4,
                    MaximumCycles = 100,
                    AbandonmentLimit = 1000
                },
                new ArraySolutionCloner<double>(),
                new MaxEvaluationsStoppingCriterion(5),
                new OptimizationOptions { Seed = 23UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void ConstantObjectiveTriggersOneScoutAtLimit()
    {
        OptimizationResult<double[]> result =
            new ArtificialBeeColonyOptimizer().Optimize(
                CreateConstantProblem(2),
                new ArtificialBeeColonyParameters
                {
                    FoodSourceCount = 4,
                    MaximumCycles = 1,
                    AbandonmentLimit = 1
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 7UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(13, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
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
    public void FactoryCreatesArtificialBeeColony()
    {
        ArtificialBeeColonyOptimizer optimizer =
            MetaheuristicFactory.Create<ArtificialBeeColonyOptimizer>(
                MetaheuristicAlgorithmIds.ArtificialBeeColony);

        Assert.NotNull(
            optimizer);
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new ArtificialBeeColonyOptimizer().Optimize(
            CreateSphere(5),
            new ArtificialBeeColonyParameters
            {
                FoodSourceCount = 8,
                MaximumCycles = 6,
                AbandonmentLimit = 40
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 12345UL },
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem
        CreateSphere(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -10.0,
                10.0),
            OptimizationSense.Minimize,
            Sphere);

    private static ContinuousOptimizationProblem
        CreateConstantProblem(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -10.0,
                10.0),
            OptimizationSense.Minimize,
            static _ => 1.0);

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
