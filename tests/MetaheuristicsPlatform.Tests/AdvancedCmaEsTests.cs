using MetaheuristicsPlatform.Algorithms.CMAES;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedCmaEsTests
{
    [Fact]
    public void ActiveDescriptorUsesWeightedNegativeReference()
    {
        var optimizer = new ActiveCmaEsOptimizer();

        Assert.Equal(
            "active-cma-es-hansen-ros-2010",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1145/1830761.1830788");
    }

    [Fact]
    public void SeparableDescriptorUsesRosHansenReference()
    {
        var optimizer = new SeparableCmaEsOptimizer();

        Assert.Equal(
            "separable-cma-es-ros-hansen-2008",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1007/978-3-540-87700-4_30");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EvaluationBudgetStopsInsideGenerationWithoutOvershoot(
        bool active)
    {
        var parameters =
            new CmaEsParameters
            {
                PopulationSize = 10,
                ParentCount = 5,
                MaximumGenerations = 50,
                InitialStepSize = 1.0
            };

        OptimizationResult<double[]> result =
            active
                ? new ActiveCmaEsOptimizer().Optimize(
                    CreateSphere(4),
                    parameters,
                    new ArraySolutionCloner<double>(),
                    new MaxEvaluationsStoppingCriterion(7),
                    new OptimizationOptions { Seed = 17UL },
                    cancellationToken:
                        TestContext.Current.CancellationToken)
                : new SeparableCmaEsOptimizer().Optimize(
                    CreateSphere(4),
                    parameters,
                    new ArraySolutionCloner<double>(),
                    new MaxEvaluationsStoppingCriterion(7),
                    new OptimizationOptions { Seed = 17UL },
                    cancellationToken:
                        TestContext.Current.CancellationToken);

        Assert.Equal(7, result.Statistics.Evaluations);
        Assert.Equal(0, result.Statistics.Iterations);
        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void ActiveDefaultOddPopulationCompletesOneGeneration()
    {
        OptimizationResult<double[]> result =
            new ActiveCmaEsOptimizer().Optimize(
                CreateSphere(3),
                new CmaEsParameters
                {
                    MaximumGenerations = 1,
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 77UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        // lambda = 4 + floor(3 ln(3)) = 7.
        Assert.Equal(7, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumActiveCmaEsGenerations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void ActiveSameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            RunActive();

        OptimizationResult<double[]> second =
            RunActive();

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
    }

    [Fact]
    public void SeparableSameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first =
            RunSeparable();

        OptimizationResult<double[]> second =
            RunSeparable();

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
    }

    [Fact]
    public void AdvancedIdsAreRegisteredByFactory()
    {
        ActiveCmaEsOptimizer active =
            MetaheuristicFactory.Create<ActiveCmaEsOptimizer>(
                MetaheuristicAlgorithmIds.ActiveCmaEs);

        SeparableCmaEsOptimizer separable =
            MetaheuristicFactory.Create<SeparableCmaEsOptimizer>(
                MetaheuristicAlgorithmIds.SeparableCmaEs);

        Assert.NotNull(active);
        Assert.NotNull(separable);
    }

    [Fact]
    public void ActiveRequiresAtLeastOneNonParentOffspring()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ActiveCmaEsOptimizer().Optimize(
                CreateSphere(3),
                new CmaEsParameters
                {
                    PopulationSize = 4,
                    ParentCount = 4,
                    MaximumGenerations = 1,
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 1UL },
                cancellationToken:
                    TestContext.Current.CancellationToken));
    }

    private static OptimizationResult<double[]> RunActive() =>
        new ActiveCmaEsOptimizer().Optimize(
            CreateRotatedLikeQuadratic(5),
            new CmaEsParameters
            {
                PopulationSize = 10,
                ParentCount = 5,
                MaximumGenerations = 6,
                InitialStepSize = 1.0
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 1234UL },
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static OptimizationResult<double[]> RunSeparable() =>
        new SeparableCmaEsOptimizer().Optimize(
            CreateSphere(5),
            new CmaEsParameters
            {
                PopulationSize = 10,
                ParentCount = 5,
                MaximumGenerations = 6,
                InitialStepSize = 1.0
            },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = 1234UL },
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
        CreateRotatedLikeQuadratic(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(
                dimension,
                -10.0,
                10.0),
            OptimizationSense.Minimize,
            RotatedLikeQuadratic);

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

    private static double RotatedLikeQuadratic(
        ReadOnlySpan<double> x)
    {
        double sum = 0.0;

        for (int i = 0; i < x.Length; i++)
        {
            double coupled =
                x[i] +
                (i == 0
                    ? 0.0
                    : 0.35 * x[i - 1]);

            sum +=
                (1.0 + i) *
                coupled *
                coupled;
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
