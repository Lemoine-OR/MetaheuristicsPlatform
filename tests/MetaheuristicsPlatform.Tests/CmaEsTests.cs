using MetaheuristicsPlatform.Algorithms.CMAES;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class CmaEsTests
{
    [Fact]
    public void DescriptorUsesCanonicalStableIdAndReference()
    {
        var optimizer =
            new CmaEsOptimizer();

        Assert.Equal(
            "cma-es-hansen-ostermeier-2001",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1162/106365601750190398");
    }

    [Fact]
    public void DefaultParametersAreValid()
    {
        new CmaEsParameters().Validate();
    }

    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CmaEsParameters
            {
                PopulationSize = -1
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CmaEsParameters
            {
                MaximumGenerations = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CmaEsParameters
            {
                InitialStepSize = 0.0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CmaEsParameters
            {
                MinimumCovarianceEigenvalue = 0.0
            }.Validate());
    }

    [Fact]
    public void CompleteGenerationsUseExactlyLambdaEvaluations()
    {
        var optimizer =
            new CmaEsOptimizer();

        OptimizationResult<double[]> result =
            optimizer.Optimize(
                CreateSphere(4),
                new CmaEsParameters
                {
                    PopulationSize = 8,
                    ParentCount = 4,
                    MaximumGenerations = 3,
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 42UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(24, result.Statistics.Evaluations);
        Assert.Equal(3, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumCmaEsGenerations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void EvaluationBudgetStopsInsideGenerationWithoutOvershoot()
    {
        var optimizer =
            new CmaEsOptimizer();

        OptimizationResult<double[]> result =
            optimizer.Optimize(
                CreateSphere(3),
                new CmaEsParameters
                {
                    PopulationSize = 10,
                    ParentCount = 5,
                    MaximumGenerations = 100,
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new MaxEvaluationsStoppingCriterion(5),
                new OptimizationOptions { Seed = 7UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
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

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
    }

    [Fact]
    public void CanonicalIdIsRegisteredByFactory()
    {
        CmaEsOptimizer created =
            MetaheuristicFactory.Create<CmaEsOptimizer>(
                MetaheuristicAlgorithmIds.CmaEs);

        Assert.NotNull(created);
    }

    [Fact]
    public void InitialMeanMustBelongToBoundedSearchSpace()
    {
        var optimizer =
            new CmaEsOptimizer();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => optimizer.Optimize(
                CreateSphere(2),
                new CmaEsParameters
                {
                    PopulationSize = 4,
                    ParentCount = 2,
                    MaximumGenerations = 1,
                    InitialMean = [100.0, 100.0],
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 1UL },
                cancellationToken:
                    TestContext.Current.CancellationToken));
    }

    private static OptimizationResult<double[]> RunDeterministic() =>
        new CmaEsOptimizer().Optimize(
            CreateSphere(5),
            new CmaEsParameters
            {
                PopulationSize = 10,
                ParentCount = 5,
                MaximumGenerations = 7,
                InitialStepSize = 1.5
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 123456UL },
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

    private static double Sphere(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * x[i];
        }

        return sum;
    }

    private sealed class NeverStoppingCriterion :
        IStoppingCriterion
    {
        public string Name => "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }
}
