using MetaheuristicsPlatform.Algorithms.BiogeographyBasedOptimization;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class BBOScientificTests
{
    [Fact]
    public void DescriptorAndFactoryUseCanonicalScientificIdentity()
    {
        var optimizer =
            new BiogeographyBasedOptimizationOptimizer();

        Assert.Equal(
            MetaheuristicAlgorithmIds.BiogeographyBasedOptimization,
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/TEVC.2008.919004");

        Assert.NotNull(
            MetaheuristicFactory.Create<BiogeographyBasedOptimizationOptimizer>(
                MetaheuristicAlgorithmIds.BiogeographyBasedOptimization));
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            Run(12345UL);

        OptimizationResult<double[]> second =
            Run(12345UL);

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);

        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);

        Assert.Equal(
            first.Statistics.Iterations,
            second.Statistics.Iterations);
    }

    [Fact]
    public void OneCompleteIterationHasExactEvaluationAccounting()
    {
        OptimizationResult<double[]> result =
            new BiogeographyBasedOptimizationOptimizer().Optimize(
                CreateSphere(4),
                new BiogeographyBasedOptimizationParameters
                {
                    PopulationSize = 6,
                    EliteCount = 2,
                    MaximumIterations = 1,
                    MaximumMutationRate = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 77UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            10,
            result.Statistics.Evaluations);

        Assert.Equal(
            1,
            result.Statistics.Iterations);
    }

    [Fact]
    public void MaximizationUsesObjectiveSenseWithoutFailure()
    {
        OptimizationResult<double[]> result =
            new BiogeographyBasedOptimizationOptimizer().Optimize(
                CreateLinearMaximizationProblem(4),
                new BiogeographyBasedOptimizationParameters
                {
                    PopulationSize = 6,
                    EliteCount = 2,
                    MaximumIterations = 1,
                    MaximumMutationRate = 0.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 91UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.True(double.IsFinite(result.BestFitness));
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void InvalidScientificControlsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new BiogeographyBasedOptimizationParameters { PopulationSize = 2 }.Validate());
    }

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new BiogeographyBasedOptimizationOptimizer().Optimize(
            CreateSphere(5),
            new BiogeographyBasedOptimizationParameters
                {
                    PopulationSize = 6,
                    EliteCount = 2,
                    MaximumIterations = 2,
                    MaximumMutationRate = 0.0
                },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = seed },
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem CreateLinearMaximizationProblem(
        int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Maximize,
            static x => x[0]);

    private static ContinuousOptimizationProblem CreateSphere(
        int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -5.0,
                5.0),
            OptimizationSense.Minimize,
            Sphere);

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
            sum += x[i] * x[i];

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
            StoppingDecision.Continue(Name);
    }
}
