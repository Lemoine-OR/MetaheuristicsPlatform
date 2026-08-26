using MetaheuristicsPlatform.Algorithms.CrowSearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class CSAScientificTests
{
    [Fact]
    public void DescriptorAndFactoryUseCanonicalScientificIdentity()
    {
        var optimizer = new CrowSearchOptimizer();
        Assert.Equal(MetaheuristicAlgorithmIds.CrowSearch, optimizer.Descriptor.Id);
        Assert.Contains(optimizer.Descriptor.References, reference => reference.Doi == "10.1016/j.compstruc.2016.03.001");
        Assert.NotNull(MetaheuristicFactory.Create<CrowSearchOptimizer>(MetaheuristicAlgorithmIds.CrowSearch));
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first = Run(12345UL);
        OptimizationResult<double[]> second = Run(12345UL);
        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(first.Statistics.Evaluations, second.Statistics.Evaluations);
        Assert.Equal(first.Statistics.Iterations, second.Statistics.Iterations);
    }

    [Fact]
    public void OneCompleteIterationHasValidatedEvaluationAccounting()
    {
        OptimizationResult<double[]> result =
            new CrowSearchOptimizer().Optimize(
                CreateSphere(4),
                new CrowSearchParameters { PopulationSize = 6, MaximumIterations = 1, FlightLength = 2.0, AwarenessProbability = 0.1 },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 77UL },
                cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(12, result.Statistics.Evaluations);
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void MaximizationUsesObjectiveSenseWithoutFailure()
    {
        OptimizationResult<double[]> result =
            new CrowSearchOptimizer().Optimize(
                CreateLinearMaximizationProblem(4),
                new CrowSearchParameters { PopulationSize = 6, MaximumIterations = 1, FlightLength = 2.0, AwarenessProbability = 0.1 },
                new ArraySolutionCloner<double>(),
                new NeverStoppingCriterion(),
                new OptimizationOptions { Seed = 91UL },
                cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(double.IsFinite(result.BestFitness));
        Assert.Equal(1, result.Statistics.Iterations);
    }

    [Fact]
    public void InvalidScientificControlsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CrowSearchParameters { AwarenessProbability = 1.1 }.Validate());
    }

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new CrowSearchOptimizer().Optimize(
            CreateSphere(5),
            new CrowSearchParameters { PopulationSize = 6, MaximumIterations = 2, FlightLength = 2.0, AwarenessProbability = 0.1 },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = seed },
            cancellationToken: TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem CreateLinearMaximizationProblem(int dimension) =>
        new(BoundedContinuousSearchSpace.Uniform(dimension, -5.0, 5.0), OptimizationSense.Maximize, static x => x[0]);

    private static ContinuousOptimizationProblem CreateSphere(int dimension) =>
        new(BoundedContinuousSearchSpace.Uniform(dimension, -5.0, 5.0), OptimizationSense.Minimize, Sphere);

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
        return sum;
    }

    private sealed class NeverStoppingCriterion : IStoppingCriterion
    {
        public string Name => "Never";
        public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }
}
