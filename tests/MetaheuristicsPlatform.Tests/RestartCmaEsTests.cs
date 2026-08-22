using MetaheuristicsPlatform.Algorithms.CMAES;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class RestartCmaEsTests
{
    [Fact]
    public void IpopDescriptorUsesCanonicalRestartReference()
    {
        var optimizer = new IpopCmaEsOptimizer();

        Assert.Equal(
            "ipop-cma-es-auger-hansen-2005",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/CEC.2005.1554902");
    }

    [Fact]
    public void BipopDescriptorUsesCanonicalBiPopulationReference()
    {
        var optimizer = new BipopCmaEsOptimizer();

        Assert.Equal(
            "bipop-cma-es-hansen-2009",
            optimizer.Descriptor.Id);

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1145/1570256.1570333");
    }

    [Fact]
    public void IpopDoublesPopulationAcrossRestarts()
    {
        OptimizationResult<double[]> result =
            new IpopCmaEsOptimizer().Optimize(
                CreateSphere(3),
                new RestartCmaEsParameters
                {
                    InitialPopulationSize = 4,
                    MaximumRestarts = 2,
                    MaximumGenerationsPerRestart = 1,
                    PopulationMultiplier = 2.0,
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 17UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        // 4 + 8 + 16 offspring evaluations.
        Assert.Equal(28, result.Statistics.Evaluations);
        Assert.Equal(3, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumIpopCmaEsRestarts",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void BipopBalancesSmallAndLargeBudgets()
    {
        OptimizationResult<double[]> result =
            new BipopCmaEsOptimizer().Optimize(
                CreateSphere(3),
                new RestartCmaEsParameters
                {
                    InitialPopulationSize = 4,
                    MaximumRestarts = 3,
                    MaximumGenerationsPerRestart = 1,
                    PopulationMultiplier = 2.0,
                    InitialStepSize = 1.0
                },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 99UL },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        // initial/small budget: 4; first large: 8;
        // next small is in [4,4] because lambda_large=8;
        // budgets tie at 8, therefore the next run is large with lambda=16.
        Assert.Equal(32, result.Statistics.Evaluations);
        Assert.Equal(4, result.Statistics.Iterations);
        Assert.Equal(
            "MaximumBipopCmaEsRestarts",
            result.StopDecision.Criterion);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GlobalEvaluationBudgetNeverOvershootsAcrossRestarts(
        bool bipop)
    {
        RestartCmaEsParameters parameters =
            new()
            {
                InitialPopulationSize = 4,
                MaximumRestarts = 5,
                MaximumGenerationsPerRestart = 2,
                InitialStepSize = 1.0
            };

        OptimizationResult<double[]> result =
            bipop
                ? new BipopCmaEsOptimizer().Optimize(
                    CreateSphere(3),
                    parameters,
                    new ArraySolutionCloner<double>(),
                    new MaxEvaluationsStoppingCriterion(7),
                    new OptimizationOptions { Seed = 3UL },
                    cancellationToken:
                        TestContext.Current.CancellationToken)
                : new IpopCmaEsOptimizer().Optimize(
                    CreateSphere(3),
                    parameters,
                    new ArraySolutionCloner<double>(),
                    new MaxEvaluationsStoppingCriterion(7),
                    new OptimizationOptions { Seed = 3UL },
                    cancellationToken:
                        TestContext.Current.CancellationToken);

        Assert.Equal(7, result.Statistics.Evaluations);
        Assert.Equal(
            "MaxEvaluations",
            result.StopDecision.Criterion);
    }

    [Fact]
    public void BipopIsDeterministicForSameSeed()
    {
        OptimizationResult<double[]> first =
            RunBipop();

        OptimizationResult<double[]> second =
            RunBipop();

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(first.Statistics.Evaluations, second.Statistics.Evaluations);
    }

    [Fact]
    public void RestartIdsAreRegisteredByFactory()
    {
        IpopCmaEsOptimizer ipop =
            MetaheuristicFactory.Create<IpopCmaEsOptimizer>(
                MetaheuristicAlgorithmIds.IpopCmaEs);

        BipopCmaEsOptimizer bipop =
            MetaheuristicFactory.Create<BipopCmaEsOptimizer>(
                MetaheuristicAlgorithmIds.BipopCmaEs);

        Assert.NotNull(ipop);
        Assert.NotNull(bipop);
    }

    private static OptimizationResult<double[]> RunBipop() =>
        new BipopCmaEsOptimizer().Optimize(
            CreateSphere(5),
            new RestartCmaEsParameters
            {
                InitialPopulationSize = 6,
                MaximumRestarts = 3,
                MaximumGenerationsPerRestart = 2,
                InitialStepSize = 1.0
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
