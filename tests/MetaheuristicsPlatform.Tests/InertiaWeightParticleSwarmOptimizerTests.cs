using MetaheuristicsPlatform.Algorithms.PSO.Scientific;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class InertiaWeightPsoScientificTests
{
    [Fact]
    public void DescriptorFactoryAndDoiUseCanonicalIdentity()
    {
        var optimizer = new InertiaWeightParticleSwarmOptimizer();
        Assert.Equal(MetaheuristicAlgorithmIds.InertiaWeightParticleSwarm, optimizer.Descriptor.Id);
        Assert.Contains(optimizer.Descriptor.References, reference => reference.Doi == "10.1109/ICEC.1998.699146");
        Assert.NotNull(MetaheuristicFactory.Create<InertiaWeightParticleSwarmOptimizer>(MetaheuristicAlgorithmIds.InertiaWeightParticleSwarm));
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

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new InertiaWeightParticleSwarmOptimizer().Optimize(
            CreateSphere(6),
            new InertiaWeightPsoParameters { SwarmSize = 6, MaximumIterations = 2 },
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
