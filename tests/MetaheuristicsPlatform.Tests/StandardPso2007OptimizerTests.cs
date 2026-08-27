using MetaheuristicsPlatform.Algorithms.PSO.Standard2007;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class StandardPso2007ScientificTests
{
    [Fact]
    public void DescriptorFactoryAndDoiUseCanonicalIdentity()
    {
        var optimizer = new StandardPso2007Optimizer();
        Assert.Equal(MetaheuristicAlgorithmIds.StandardParticleSwarm2007, optimizer.Descriptor.Id);
        Assert.Contains(optimizer.Descriptor.References, reference => reference.Doi == "10.1109/SIS.2007.368035");
        Assert.NotNull(MetaheuristicFactory.Create<StandardPso2007Optimizer>(MetaheuristicAlgorithmIds.StandardParticleSwarm2007));
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
    public void PublishedStandardParameterizationIsPreserved()
    {
        var parameters = new StandardPso2007Parameters();
        Assert.Equal(1.0 / (2.0 * Math.Log(2.0)), parameters.InertiaWeight, 12);
        Assert.Equal(0.5 + Math.Log(2.0), parameters.AccelerationCoefficient, 12);
        Assert.Equal(14, parameters.ResolveSwarmSize(4));
        Assert.Equal(3, parameters.ExpectedInformerCount);
    }

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new StandardPso2007Optimizer().Optimize(
            CreateSphere(6),
            new StandardPso2007Parameters { SwarmSize = 6, ExpectedInformerCount = 3, MaximumIterations = 2 },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = seed },
            cancellationToken: TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem CreateSphere(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(dimension, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
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
