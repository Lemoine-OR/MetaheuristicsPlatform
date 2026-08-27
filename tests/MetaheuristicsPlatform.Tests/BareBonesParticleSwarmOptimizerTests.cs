using MetaheuristicsPlatform.Algorithms.PSO.BareBones;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class BareBonesPsoScientificTests
{
    [Fact]
    public void DescriptorFactoryAndDoiUseCanonicalIdentity()
    {
        var optimizer = new BareBonesParticleSwarmOptimizer();
        Assert.Equal(MetaheuristicAlgorithmIds.BareBonesParticleSwarm, optimizer.Descriptor.Id);
        Assert.Contains(optimizer.Descriptor.References, reference => reference.Doi == "10.1109/SIS.2003.1202251");
        Assert.NotNull(MetaheuristicFactory.Create<BareBonesParticleSwarmOptimizer>(MetaheuristicAlgorithmIds.BareBonesParticleSwarm));
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
    public void GaussianDistributionMatchesPublishedCenterAndSpread()
    {
        BareBonesPsoDistribution distribution = BareBonesPsoDistribution.From(2.0, 8.0);
        Assert.Equal(5.0, distribution.Mean);
        Assert.Equal(6.0, distribution.StandardDeviation);
    }

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new BareBonesParticleSwarmOptimizer().Optimize(
            CreateSphere(6),
            new BareBonesPsoParameters { SwarmSize = 6, MaximumIterations = 2 },
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
